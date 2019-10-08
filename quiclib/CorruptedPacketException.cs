using System;

namespace quicsharp
{
    [Serializable]
    public class CorruptedPacketException : Exception
    {
        public CorruptedPacketException()
        {

        }

        public CorruptedPacketException(string description)
            : base(String.Format("Corrupted QUIC packet {0}", description))
        {

        }
    }
}
