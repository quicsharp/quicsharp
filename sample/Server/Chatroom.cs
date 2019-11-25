using System;
using System.Collections.Generic;
using System.Text;
using quicsharp;

namespace Server
{
    public class Chatroom
    {
        public List<QuicConnection> Connections = new List<QuicConnection>();

        /// <summary>
        /// Adds newConnection to the List of connections in the chatroom
        /// </summary>
        /// <param name="newConnection"></param>
        public void addConnection(QuicConnection newConnection)
        {
            Connections.Add(newConnection);
        }
        
        /// <summary>
        /// Remove a connection from the chatroom
        /// </summary>
        /// <param name="connection"></param>
        public void removeConnection(QuicConnection connection)
        {
            Connections.Remove(connection);
        }

        /// <summary>
        /// Check whether a connection is in the chatroom or not
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public bool containsConnection(QuicConnection connection)
        {
            return Connections.Contains(connection);
        }
    }
}
