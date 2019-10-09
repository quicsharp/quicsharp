using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

using quicsharp.Frames;

namespace quicsharp
{
    public class QuicClient
    {
        private UdpClient client_;
        private UInt32 packetNumber_;

        public QuicClient()
        {
            client_ = new UdpClient();
            packetNumber_ = 0;
        }

        // Connect to a remote server.
        public void Connect(string ip, int port)
        {
            InitialPacket initialPacket = new InitialPacket(0, 0, packetNumber_++);

            byte[] byteInitialPacket = initialPacket.Encode();
            client_.Send(byteInitialPacket, byteInitialPacket.Length, ip, port);

            IPEndPoint server = null;
            Packet packet = Packet.Unpack(client_.Receive(ref server));

            if (packet.GetType() == typeof(InitialPacket))
            {
                Console.WriteLine("New initial packet");
                packet.DecodeFrames();
                Console.WriteLine("Data received {0}:{1}.", server.Address, server.Port);

                InitialPacket initPack = packet as InitialPacket;
                Console.WriteLine($"I am client n {initPack.DCID} connected to server n {initPack.SCID}");
            }
        }

        public int Send(byte[] payload)
        {
            packetNumber_++;

            return 0;
        }

        public void Close()
        {
            client_.Close();
        }
    }
}
