using System;

namespace quicsharp
{
    /// <summary>
    /// Custom exception to handle correupted quic packets
    /// </summary>
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
