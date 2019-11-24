using System;
using System.Net;
using System.Net.Sockets;

namespace quicsharp
{
    /// <summary>
    /// QuicConnection that handle client information.
    /// </summary>
    public class QuicConnectionFromClient : QuicConnection
    {
        public QuicConnectionFromClient(UdpClient server, IPEndPoint endpoint, byte[] connID, byte[] peerID) : base(server, endpoint, connID, peerID)
        {

        }
    }
}
