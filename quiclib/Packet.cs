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
                if (!Packet.ReadBit(1, data))
                    throw new ArgumentException("Corrupted packet");
                switch (Packet.ReadNBits(2, data, 2))
                {
                    case 0:
                        p = new InitialPacket();
                        break;
                    case 1:
                        p = new RTTPacket();
                        break;
                    case 2:
                        p = new HandshakePacket();
                        break;
                    case 3:
                        p = new RetryPacket();
                        break;
                    default:
                        p = new LongHeaderPacket();
                        Console.WriteLine("Congrats to anyone managing to encode any value other than 0, 1, 2 or 3 on 2 bits");
                        break;
                }
                p.Decode(data);
            }

            return p;
        }

        public static void WriteBit(int index, byte[] data, bool b)
        {
            if (data.Length <= index / 8)
                throw new AccessViolationException("QUIC packet too small");


            if (!b)
            {
                byte andNumber = Convert.ToByte(0xFF);
                andNumber -= Convert.ToByte(1 << (7 - (index % 8)));
                data[index / 8] = Convert.ToByte(data[index / 8] & andNumber);
            }
            else
            {
                byte orNumber = 0;
                orNumber += Convert.ToByte(1 << (7 - (index % 8)));
                data[index / 8] = Convert.ToByte(data[index / 8] | orNumber);
            }
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
            return (data[index / 8] >> (index % 8)) % 2 == 1;
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

        public static int ReadByte(int indexBegin, byte[] data)
        {
            return ReadNBits(indexBegin, data, 8);
        }

        public static int ReadNBytes(int indexBegin, byte[] data, int n)
        {
            return ReadNBits(indexBegin, data, n*8);
        }

        public static UInt32 ReadUInt32(int indexBegin, byte[] data)
        {
            UInt32 ret = (UInt32)Packet.ReadNBits(indexBegin, data, 32);

            return ret;
        }
        public static UInt64 ReadUInt64(int indexBegin, byte[] data)
        {
            UInt64 ret = (UInt64)Packet.ReadNBits(indexBegin, data, 64);

            return ret;
        }

        public static (int, dynamic) ReadVariableLengthInteger(int indexBegin, byte[] data)
        {
            // See section 16 of QUIC IETF Draft on variable-length integer encoding
            switch(ReadNBits(indexBegin, data, 2))
            {
                case 0:
                    return (indexBegin + 8, ReadByte(indexBegin, data));
                case 1:
                    return (indexBegin + 16, ReadNBits(indexBegin, data, 16) % (1 << 14));
                case 2:
                    return (indexBegin + 32, ReadUInt32(indexBegin, data) % (1 << 30));
                case 3:
                    return (indexBegin + 64, ReadUInt64(indexBegin, data) % (1 << 62));
                default:
                    break;
            }
            throw new ArgumentException("2 Bit-encoded uint is not amongst {0, 1, 2, 3} : mathematics are broken and life is lawless");
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
