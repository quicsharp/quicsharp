using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace quicsharp
{
    class Packet
    {
        public byte[] Payload;
        public UInt32 ClientId;
        // Byte
        private static int packetHeaderSize_ = 4;

        public static Packet Unpack(byte[] data)
        {
            if (data.Length < packetHeaderSize_)
            {
                throw new ArgumentException("Corrupted packet");
            }
            Packet p = new Packet();
            p.ClientId = BitConverter.ToUInt32(data, 0);

            p.Payload = new byte[data.Length - packetHeaderSize_];
            Array.Copy(data, 4, p.Payload, 0, p.Payload.Length);

            return p;
        }
    }
}
