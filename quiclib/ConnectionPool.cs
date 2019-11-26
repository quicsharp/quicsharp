using System;
using System.Collections.Generic;

namespace quicsharp
{
    /// <summary>
    /// A pool of QuicConnection used by the QuicListener to manage all the opened connections
    /// </summary>
    public class ConnectionPool
    {
        private Dictionary<string, QuicConnection> _pool = new Dictionary<string, QuicConnection>();

        /// <summary>
        /// Add a connection to the connection pool
        /// </summary>
        /// <param name="connection">The connection to add</param>
        /// <param name="connID">ID of the connection peer (= SCID or DCID)</param>
        public void AddConnection(QuicConnection connection, byte[] connID)
        {
            string strConnID = BitConverter.ToString(connID);
            _pool.Add(strConnID, connection);

            Logger.Write($"Connection {strConnID} added to the ConnectionPool");
        }

        /// <summary>
        /// Remove a connection from the connection pool
        /// Used when a connection with the server is closed
        /// </summary>
        /// <param name="connID">ID of the connection peer (= SCID or DCID) to remove</param>
        public void RemoveConnection(byte[] connID)
        {
            string strConnID = BitConverter.ToString(connID);

            if (_pool.ContainsKey(strConnID))
            {
                _pool.Remove(strConnID);
                Logger.Write($"Connection #{strConnID} removed from the ConnectionPool");
            }
        }

        /// <summary>
        /// Return the connection related to the connID
        /// </summary>
        /// <param name="connID">ID of the connection peer (= SCID or DCID) to find</param>
        /// <returns>The connection instance</returns>
        public QuicConnection Find(byte[] connID)
        {
            string strConnID = BitConverter.ToString(connID);

            if (_pool.ContainsKey(strConnID) == false)
                return null;

            return _pool[strConnID];
        }

        public List<QuicConnection> GetPool()
        {
            List<QuicConnection> list = new List<QuicConnection>();
            foreach(KeyValuePair<string, QuicConnection> pair in _pool)
            {
                list.Add(pair.Value);
            }
            return list;
        }
    }
}
