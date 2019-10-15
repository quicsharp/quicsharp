using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace quicsharp
{
    public class QuicServerConnection : QuicConnection
    {
        public QuicServerConnection(UdpClient server, IPEndPoint endpoint, byte[] clientId, byte[] serverId) : base(server, endpoint, clientId, serverId)
        {
        }
    }
}
