using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

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
            client_.Send(Encoding.ASCII.GetBytes("Hello"), 5, ip, port);
        }

        public void Close()
        {
            client_.Close();
        }
    }
}
