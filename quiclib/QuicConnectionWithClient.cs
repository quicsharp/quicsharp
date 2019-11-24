using System;
using System.Net;
using System.Net.Sockets;

namespace quicsharp
{
    /// <summary>
    /// QuicConnection that handle client information.
    /// </summary>
    public class QuicConnectionWithClient : QuicConnection
    {
        public QuicConnectionWithClient(UdpClient server, IPEndPoint endpoint, byte[] connID, byte[] peerID) : base(server, endpoint, connID, peerID)
        {

        }
    }
}
