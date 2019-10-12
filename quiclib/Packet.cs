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

        public UInt32 PacketNumber = 0;

        // Byte
        protected static int packetHeaderSize_ = 4;

        public static Packet Unpack(byte[] data)
        {
            if (data.Length < packetHeaderSize_)
            {
                throw new ArgumentException("Corrupted packet");
            }
            Packet p;

            if (!BitUtils.ReadBit(0, data))
            {
                // Short Header Packet
                p = new ShortHeaderPacket();
                if (!BitUtils.ReadBit(1, data))
                    throw new ArgumentException("Corrupted packet");

                p.Decode(data);
            }
            else
            {
                // Long Header Packet
                if (!BitUtils.ReadBit(1, data))
                    throw new ArgumentException("Corrupted packet");
                switch (BitUtils.ReadNBits(2, data, 2))
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
            FrameParser fp = new FrameParser(Payload);
            if (Payload.Length == 0)
                return;
            //    throw new ArgumentException("The payload is empty. Can't decode frames");

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
