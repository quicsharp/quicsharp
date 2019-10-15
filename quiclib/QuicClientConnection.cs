using System;
using System.Net;
using System.Net.Sockets;

namespace quicsharp
{
    public class QuicClientConnection : QuicConnection
    {
        public QuicClientConnection(UdpClient server, IPEndPoint endpoint, byte[] clientId, byte[] serverId) : base(server, endpoint, serverId, clientId)
        {

        }
    }
}
