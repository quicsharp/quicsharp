using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace quicsharp
{
    public class Packet
    {
        public byte[] Payload;
        public UInt32 ClientId;
        public List<Frame> Frames { get; protected set; } = new List<Frame>();

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
                        throw new ArgumentException("2 Bit-encoded uint is not amongst {0, 1, 2, 3} : mathematics are broken and life is lawless");
                }
                p.Decode(data);
            }

            return p;
        }

        public virtual void DecodeFrames()
        {
            if (Payload.Length == 0)
                throw new ArgumentException("The payload is empty. Can't decode frames");

            FrameParser fp = new FrameParser(Payload);

            Frames = fp.GetFrames();
        }

        public virtual void AddFrame(Frame frame)
        {
            Frames.Add(frame);
        }

        public virtual byte[] EncodeFrames()
        {
            List<byte> result = new List<byte>();
            foreach (Frame frame in Frames)
            {
                result.AddRange(frame.Encode());
            }

            return result.ToArray();
        }

        public static void WriteBit(int index, byte[] data, bool b)
        {
            if (data.Length <= index / 8)
                throw new AccessViolationException($"QUIC packet too small (size: {data.Length * 8}, reading at: {index})");

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
                throw new AccessViolationException($"QUIC packet too small (size: {data.Length * 8}, reading at: {indexBegin})");

            for (int i = 0; i < b.Length; i++)
            {
                WriteBit(indexBegin + i, data, b[i]);
            }
        }

        public static void WriteUInt32(int indexBegin, byte[] data, UInt32 toWrite)
        {
            if (data.Length < (indexBegin / 8) + 4)
                throw new AccessViolationException($"QUIC packet too small (size: {data.Length * 8}, reading at: {indexBegin})");

            for (int i = 0; i < 32; i++)
            {
                bool b = ((toWrite >> i) % 2 == 1);
                WriteBit(indexBegin + 31 -  i, data, b);
            }
        }

        public static void WriteNByteFromInt(int indexBegin, byte[] data, uint toWrite, int n)
        {
            if (data.Length < (indexBegin / 8) + n)
                throw new AccessViolationException($"QUIC packet too small (size: {data.Length * 8}, reading at: {indexBegin})");

            if (toWrite > Math.Pow(2, (8*n)) - 1)
                throw new ArgumentException($"The following int can not be converted into {n} byte : {toWrite}");

            for (int i = 0; i < 8*n; i++)
            {
                bool b = ((toWrite >> i) % 2 == 1);
                WriteBit(indexBegin + (8 * n) - 1 - i, data, b);
            }
        }

        public static bool ReadBit(int index, byte[] data)
        {
            if (data.Length <= index / 8)
                throw new AccessViolationException($"QUIC packet too small (size: {data.Length * 8}, reading at: {index})");

            // True if the bit n index is true
            return (data[index / 8] >> (7 - (index % 8))) % 2 == 1;
        }

        public static int ReadNBits(int indexBegin, byte[] data, int n)
        {
            int ret = 0;

            if (data.Length < (indexBegin + n)/ 8)
                throw new AccessViolationException($"QUIC packet too small (size: {data.Length * 8}, reading at: {indexBegin} + {n})");

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
