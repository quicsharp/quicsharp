using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp
{
    class ConnectionPool
    {
        private static int maxConnection_ = 16777216;

        private static UInt32 connectionId_ = 0;

        private static Dictionary<UInt32, QuicConnection> _pool = new Dictionary<UInt32, QuicConnection>();

        private static List<QuicConnection> _draining = new List<QuicConnection>();

        public static bool AddConnection(QuicConnection connection)
        { 
            if (_pool.ContainsKey(connectionId_))
                return false;

            if (_pool.Count > maxConnection_)
                return false;

            // TODO : give correct ID ; Does not work when removing connection
            _pool.Add(connectionId_, connection);

            Console.WriteLine("Connection added id: {0}", connectionId_);
            connectionId_++;

            return true;
        }

        public static void RemoveConnection(UInt32 id)
        {
            if (_pool.ContainsKey(id))
            {
                _pool.Remove(id);
                Console.WriteLine("Connection removed id: {0}", id);
            }
        }

        public static QuicConnection Find(UInt32 id)
        {
            if (_pool.ContainsKey(id) == false)
                return null;

            return _pool[id];
        }
    }
}
