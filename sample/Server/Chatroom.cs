using System;
using System.Collections.Generic;
using System.Text;
using quicsharp;

namespace Server
{
    public class Chatroom
    {
        List<QuicClientConnection> Connections = new List<QuicClientConnection>();

        public void addConnection(QuicClientConnection newConnection)
        {
            Connections.Add(newConnection);
        }
        
        public void removeConnection(QuicClientConnection connection)
        {
            Connections.Remove(connection);
        }

        public bool containsConnection(QuicClientConnection connection)
        {
            return Connections.Contains(connection);
        }
    }
}
