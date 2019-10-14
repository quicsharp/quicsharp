using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace quicsharp
{
    public class QuicConnection
    {
        public IPEndPoint EndPoint;

        private PacketManager packetManager_;
        private Dictionary<UInt64, QuicStream> streams_;

        public QuicConnection(IPEndPoint client)
        {
            EndPoint = client;
            streams_ = new Dictionary<UInt64, QuicStream>();
        }

        public QuicStream CreateStream(VariableLengthInteger id, byte type)
        {
            QuicStream stream = new QuicStream(this, id, type);
            streams_.Add(id.Value, stream);

            return stream;
        }
    }
}
