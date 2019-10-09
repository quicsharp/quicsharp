using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace quicsharp
{
    public class QuicConnection
    {
        public IPEndPoint EndPoint;

        private PacketManager packetManager_;

        public QuicConnection(IPEndPoint client)
        {
            EndPoint = client;
        }
    }
}
