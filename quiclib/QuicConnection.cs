using quicsharp.Frames;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace quicsharp
{
    public class QuicConnection
    {
        private IPEndPoint endpoint_;
        private UdpClient socket_;
        private UInt64 lastStreamId_;

        protected PacketManager packetManager_;
        // TODO : split awaiting frames by packet space
        protected Queue<Frame> awaitingFrames_;
        public List<UInt32> Received = new List<UInt32>();
        protected Dictionary<UInt64, QuicStream> streams_;

        protected Packet currentPacket_;

        public QuicConnection(UdpClient socket, IPEndPoint endPoint, byte[] scid, byte[] dcid)
        {
            socket_ = socket;
            endpoint_ = endPoint;
            streams_ = new Dictionary<UInt64, QuicStream>();
            packetManager_ = new PacketManager(scid, dcid);
            lastStreamId_ = 0;
        }

        public QuicStream CreateStream(byte type)
        {
            lastStreamId_++;
            QuicStream stream = new QuicStream(this, new VariableLengthInteger(lastStreamId_), type);
            streams_.Add(lastStreamId_, stream);

            return stream;
        }

        public void ReadPacket(Packet packet)
        {
            packet.DecodeFrames();

            foreach (Frame frame in packet.Frames)
            {
                if (frame is StreamFrame)
                {
                    StreamFrame sf = frame as StreamFrame;
                    Console.WriteLine($"Received StreamFrame with message: {System.Text.Encoding.UTF8.GetString(sf.Data)}");
                }
                if (frame is AckFrame)
                {
                    AckFrame sf = frame as AckFrame;
                    Console.WriteLine($"Received AckFrame with message: {sf.ToString()}");
                }
            }

            // Store received PacketNumber for further implementation of acknowledgement procedure
            Received.Add(packet.PacketNumber);

            // Generate a new Ack Frame and send it directly
            AckFrame ack = new AckFrame(new List<UInt32>() { packet.PacketNumber }, 100);
            AddFrame(ack);

            return ;
        }

        public int SendPacket(Packet packet)
        {
            packetManager_.PreparePacket(packet);

            byte[] data = packet.Encode();

            int sent = socket_.Send(data, data.Length, endpoint_);

            // If some bytes were sent
            return sent;
        }

        public IPEndPoint LastTransferEndpoint()
        {
            return endpoint_;
        }

        public bool SetDCID(byte[] dcid)
        {
            if (dcid != null || dcid.Length == 0)
                return false;

            packetManager_.DCID = dcid;

            return true;
        }

        // Add frame to the current packet. This current packet will be sent when the QuicConnection decides so.
        // For the moment the current packet is always a ShortHeaderPacket, and it is sent after one frame is added.
        public void AddFrame(Frame frame)
        {
            if (socket_ == null || endpoint_ == null)
                throw new NullReferenceException();
            // TODO: only ShortHeaderPacket for now
            if (currentPacket_ == null)
                // TO CHECK: use 0-RTT packet to send non encrypted data
                currentPacket_ = new RTTPacket();

            currentPacket_.AddFrame(frame);

            // TODO: decide when to send the packet
            SendCurrentPacket();
        }

        public int SendCurrentPacket()
        {
            if (currentPacket_ == null)
                throw new CorruptedPacketException();
            int sentBytes = SendPacket(currentPacket_);

            currentPacket_ = null;

            return sentBytes;
        }

        public QuicStream GetStream(UInt64 id)
        {
            if (!streams_.ContainsKey(id))
                throw new ArgumentException($"The Quic Stream id {id} does not exist");

            return streams_[id];
        }
    }
}
