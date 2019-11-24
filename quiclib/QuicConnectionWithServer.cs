using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace quicsharp
{
    /// <summary>
    /// QuicConnection that handle client information.
    /// </summary>
    public class QuicConnectionWithServer : QuicConnection
    {
        public QuicConnectionWithServer(UdpClient server, IPEndPoint endpoint, byte[] connID, byte[] peerID, Mutex mutex) : base(server, endpoint, connID, peerID)
        {

        }
    }
}
