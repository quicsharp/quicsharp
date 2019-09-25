using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace quicsharp
{
    class Packet
    {
        public byte[] PacketBytes;
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
            Packet p;

            if (!Packet.ReadBit(0, data))
            {
                // Short Header Packet
                p = new ShortHeaderPacket();
            }
            else
            {
                // Long Header Packet
                p = new LongHeaderPacker();
            }
            p.ClientId = BitConverter.ToUInt32(data, 0);

            p.Payload = new byte[data.Length - packetHeaderSize_];
            p.PacketBytes = new byte[data.Length];
            Array.Copy(data, 4, p.Payload, 0, p.Payload.Length);
            

            return p;
        }

        public static bool ReadBit(int index, byte[] data)
        {
            if (data.Length <= index / 8)
                throw new AccessViolationException("QUIC packet too small");

            // True if the bit n index is true
            return (data[index / 8] >> index % 8) % 2 == 0;
        }

        public static UInt32 ReadUInt32(int indexBegin, byte[] data)
        {
            UInt32 ret = 0;

            if (data.Length <= (indexBegin / 8) + 4)
                throw new AccessViolationException("QUIC packet too small");

            for (int i = 0; i < 32; i++)
            {
                ret = ret << 1;
                if (Packet.ReadBit(indexBegin + i, data))
                {
                    ret += 1; 
                }
            }

            return ret;
        }
    }
}
