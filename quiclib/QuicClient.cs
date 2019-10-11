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

        private ServerConnection serverConnection_;

        public bool Connected;

        public QuicClient()
        {
            client_ = new UdpClient();
            packetNumber_ = 0;
            Connected = false;
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
                serverConnection_ = new ServerConnection(new UdpClient(), server);
                Connected = true;
            }
        }

        public int Send(byte[] payload)
        {
            if (!Connected)
                return -1;
            packetNumber_++;
            
            ShortHeaderPacket packet = new ShortHeaderPacket();
            packet.AddFrame(new DebugFrame{ Message = payload.ToString() });

            return serverConnection_.SendPacket(packet);
        }

        private int Send(Packet packet)
        {
            return serverConnection_.SendPacket(packet);
        }

        public void Close()
        {
            client_.Close();
            Connected = false;
        }
    }
}
