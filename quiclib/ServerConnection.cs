using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace quicsharp
{
    class ServerConnection
    {
        private UdpClient server_;
        private IPEndPoint endpoint_;

        private PacketManager packetManager_;

        public ServerConnection(UdpClient server, IPEndPoint endpoint, UInt32 clientId, UInt32 serverId)
        {
            server_ = server;
            endpoint_ = endpoint;
            packetManager_ = new PacketManager(clientId, serverId);
        }

        public Packet ReadPacket()
        {
            // Await response for sucessfull connection creation by the server
            byte[] peerData = server_.Receive(ref endpoint_);
            if (peerData == null)
                throw new ApplicationException("QUIC Server did not respond.");

            Packet packet = new Packet{ Payload = peerData };

            return packet;
        }

        public int SendPacket(Packet packet)
        {
            byte[] data = packet.Payload;

            int sent = server_.Send(data, data.Length, endpoint_);
            
            if (packet.PacketNumber != 0)
                packetManager_.Register(packet, packet.PacketNumber);

            // If some bytes were sent
            return sent;
        }

        public IPEndPoint LastTransferEndpoint()
        {
            return endpoint_;
        }
    }
}
