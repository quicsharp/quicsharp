using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp
{
    public class ConnectionPool
    {
        private int maxConnection_ = 16777216;

        private UInt32 connectionId_ = 4096;

        private Dictionary<UInt32, QuicConnection> _pool = new Dictionary<UInt32, QuicConnection>();

        private List<QuicConnection> _draining = new List<QuicConnection>();

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

        public void RemoveConnection(UInt32 id)
        {
            if (_pool.ContainsKey(id))
            {
                _pool.Remove(id);
                Logger.Write($"Connection removed from the ConnectionPool: id = {id}");
            }
        }

        public QuicConnection Find(UInt32 id)
        {
            if (_pool.ContainsKey(id) == false)
                return null;

            return _pool[id];
        }
    }
}
