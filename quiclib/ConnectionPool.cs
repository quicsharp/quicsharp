using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp
{
    /// <summary>
    /// A pool of QuicConnection used by the QuicListener to manage all the opened connections
    /// </summary>
    public class ConnectionPool
    {
        /// <summary>
        /// Id limit for a connection
        /// </summary>
        private int maxConnection_ = 16777216;

        private UInt32 connectionId_ = 4096;

        private Dictionary<UInt32, QuicConnection> _pool = new Dictionary<UInt32, QuicConnection>();

        /// <summary>
        /// Add a connection to the connection pool
        /// </summary>
        /// <param name="connection">The connection to add</param>
        /// <returns></returns>
        public byte[] AddConnection(QuicConnection connection)
        {
            if (_pool.ContainsKey(connectionId_))
                return new byte[] { };

            if (_pool.Count > maxConnection_)
                return new byte[] { };

            // TODO : give correct ID ; Does not work when removing connection
            _pool.Add(connectionId_, connection);

            Logger.Write($"Connection added to the ConnectionPool: id = {connectionId_}");
            connectionId_++;

            return BitConverter.GetBytes(connectionId_ - 1);
        }

        /// <summary>
        /// Remove a connection from the connection pool
        /// Used when a connection with the server is closed
        /// </summary>
        /// <param name="id">The connection to remove</param>
        public void RemoveConnection(UInt32 id)
        {
            if (_pool.ContainsKey(id))
            {
                _pool.Remove(id);
                Logger.Write($"Connection removed from the ConnectionPool: id = {id}");
            }
        }

        /// <summary>
        /// Return the connection related to the client connection id
        /// </summary>
        /// <param name="id">Client connection id</param>
        /// <returns>The connection instance</returns>
        public QuicConnection Find(UInt32 id)
        {
            if (_pool.ContainsKey(id) == false)
                return null;

            return _pool[id];
        }

        public List<QuicConnection> GetPool()
        {
            List<QuicConnection> list = new List<QuicConnection>();
            foreach(KeyValuePair<uint, QuicConnection> pair in _pool)
            {
                list.Add(pair.Value);
            }
            return list;
        }
    }
}
