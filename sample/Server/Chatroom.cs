using System;
using System.Collections.Generic;
using System.Text;
using quicsharp;

namespace Server
{
    public class Chatroom
    {
        List<QuicConnection> Connections = new List<QuicConnection>();

        public void addConnection(QuicConnection newConnection)
        {
            Connections.Add(newConnection);
        }
        
        public void removeConnection(QuicConnection connection)
        {
            Connections.Remove(connection);
        }

        public bool containsConnection(QuicConnection connection)
        {
            return Connections.Contains(connection);
        }
    }
}
