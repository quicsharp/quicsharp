using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace quicsharp
{
    public class QuicServerConnection : QuicConnection
    {
        public QuicServerConnection(UdpClient server, IPEndPoint endpoint, byte[] clientId, byte[] serverId, Mutex mutex) : base(server, endpoint, clientId, serverId)
        {
            
        }
    }
}
