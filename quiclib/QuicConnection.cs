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

        protected PacketManager packetManager_;
        protected Dictionary<UInt64, QuicStream> streams_;

        protected Packet currentPacket_;

        public QuicConnection(UdpClient socket, IPEndPoint endPoint, byte[] scid, byte[] dcid)
        {
            socket_ = socket;
            endpoint_ = endPoint;
            streams_ = new Dictionary<UInt64, QuicStream>();
            packetManager_ = new PacketManager(scid, dcid);
        }

        public QuicStream CreateStream(VariableLengthInteger id, byte type)
        {
            QuicStream stream = new QuicStream(this, id, type);
            streams_.Add(id.Value, stream);

            return stream;
        }

        public Packet ReadPacket()
        {
            // Await response for sucessfull connection creation by the server
            byte[] peerData = socket_.Receive(ref endpoint_);
            if (peerData == null)
                throw new ApplicationException("QUIC Server did not respond.");

            Packet packet = new Packet { Payload = peerData };

            return packet;
        }

        public int SendPacket(Packet packet)
        {
            byte[] data = packet.Payload;

            int sent = socket_.Send(data, data.Length, endpoint_);

            if (packet.PacketNumber != 0)
                packetManager_.Register(packet, packet.PacketNumber);

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
                currentPacket_ = new ShortHeaderPacket();

            currentPacket_.AddFrame(frame);

            // TODO: decide when to send the packet
            SendCurrentPacket();
        }

        public int SendCurrentPacket()
        {
            if (currentPacket_ == null)
                throw new CorruptedPacketException();
            byte[] encodedPacket = currentPacket_.Encode();
            int sentBytes = socket_.Send(encodedPacket, encodedPacket.Length, endpoint_);

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
