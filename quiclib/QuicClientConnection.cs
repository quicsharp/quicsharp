using System;
using System.Net;
using System.Net.Sockets;

namespace quicsharp
{
    /// <summary>
    /// QuicConnection that handle client information.
    /// </summary>
    public class QuicClientConnection : QuicConnection
    {
        public QuicClientConnection(UdpClient server, IPEndPoint endpoint, byte[] clientId, byte[] serverId) : base(server, endpoint, serverId, clientId)
        {

        }
    }
}
