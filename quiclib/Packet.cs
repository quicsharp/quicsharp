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
        protected static int packetHeaderSize_ = 4;

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
                if (!Packet.ReadBit(1, data))
                    throw new ArgumentException("Corrupted packet");

                p.Decode(data);
            }
            else
            {
                // Long Header Packet
                p = new LongHeaderPacker();

                p.Decode(data);
            }

            return p;
        }

        public static void WriteBit(int index, byte[] data, bool b)
        {
            if (data.Length <= index / 8)
                throw new AccessViolationException("QUIC packet too small");

            byte andNumber = Convert.ToByte(0xFF);
            if (!b)
                andNumber -= Convert.ToByte(1 << (7 - index % 8));

            data[index / 8] = Convert.ToByte(data[index / 8] & andNumber);
        }
        public static void WriteNBits(int indexBegin, byte[] data, bool[] b)
        {
            if (data.Length <= (indexBegin / 8) + (b.Length / 8))
                throw new AccessViolationException("QUIC packet too small");
            
            for(int i = 0; i < b.Length; i++)
            {
                WriteBit(indexBegin + i, data, b[i]);
            }
        }

        public static void WriteUInt32(int indexBegin, byte[] data, UInt32 toWrite)
        {
            if (data.Length <= (indexBegin / 8) + 4)
                throw new AccessViolationException("QUIC packet too small");

            for (int i = 0; i < 32; i++)
            {
                bool b = ((toWrite >> i) % 2 == 1);
                WriteBit(indexBegin + 31 -  i, data, b);
            }
        }

        public static bool ReadBit(int index, byte[] data)
        {
            if (data.Length <= index / 8)
                throw new AccessViolationException("QUIC packet too small");

            // True if the bit n index is true
            return (data[index / 8] >> index % 8) % 2 == 0;
        }

        public static int ReadNBits(int indexBegin, byte[] data, int n)
        {
            int ret = 0;

            if (data.Length <= (indexBegin / 8) + (n / 8))
                throw new AccessViolationException("QUIC packet too small");

            for (int i = 0; i < n; i++)
            {
                ret = ret << 1;
                if (Packet.ReadBit(indexBegin + i, data))
                {
                    ret += 1;
                }
            }

            return ret;
        }

        public static UInt32 ReadUInt32(int indexBegin, byte[] data)
        {
            UInt32 ret = (UInt32)Packet.ReadNBits(indexBegin, data, 32);

            return ret;
        }

        public virtual void Decode(byte[] data)
        {
            if(data.Length < packetHeaderSize_)
                throw new AccessViolationException("QUIC packet too small");
        }

        public virtual byte[] Encode()
        {
            throw new NotImplementedException();
        }
    }
}
