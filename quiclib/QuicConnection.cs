using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace quicsharp
{
    public class QuicConnection
    {
        protected IPEndPoint endpoint_;
        protected UdpClient socket_;

        protected PacketManager packetManager_;
        protected Dictionary<UInt64, QuicStream> streams_;

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
    }
}
