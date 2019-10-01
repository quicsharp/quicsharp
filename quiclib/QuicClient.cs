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
        public QuicClient()
        {
            client_ = new UdpClient();
        }

        // Connect to a remote server.
        public void Connect(string ip, int port)
        {
            ShortHeaderPacket pack = new ShortHeaderPacket();
            pack.PacketNumber = 1;
            pack.AddFrame(new DebugFrame { Message = "Hello" });

            byte[] bytePacket = pack.Encode();

            client_.Send(bytePacket, bytePacket.Length, ip, port);
        }

        public void Close()
        {
            client_.Close();
        }
    }
}
