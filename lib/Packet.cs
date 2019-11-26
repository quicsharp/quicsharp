using System;
using System.Collections.Generic;

namespace quicsharp
{
    /// <summary>
    /// A factory used to create the correct packet type according to the received payload.
    /// </summary>
    abstract public class Packet
    {
        // Raw payload of the packet
        public byte[] Payload;
        // List of frames inside the payload
        public List<Frame> Frames { get; protected set; } = new List<Frame>();

        // Packet number (excpet for RetryPacket)
        public UInt32 PacketNumber = 0;
        // If it has a frame different from PingFrame or AckFrame
        public bool IsAckEliciting = false;

        private static int _packetHeaderSize = 4; // In bytes

        /// <summary>
        /// Factory that creates the correct Packet type according to the payload
        /// Decode the received payload
        /// </summary>
        /// <param name="data">The raw packet received</param>
        /// <returns>The decoded packet</returns>
        public static Packet Unpack(byte[] data)
        {
            if (data.Length < _packetHeaderSize)
            {
                throw new CorruptedPacketException("Wrong header size");
            }
            Packet p;

            // Short header packets starts with the bits | 0 | 1 |
            if (!BitUtils.ReadBit(0, data))
            {
                // Short Header Packet
                p = new ShortHeaderPacket();
                if (!BitUtils.ReadBit(1, data))
                    throw new CorruptedPacketException("Wrong second bit when decoding a ShortHeaderPacket");

                p.Decode(data);
            }
            // Long header packets starts with the bits | 1 | 1 |
            else
            {
                // Long Header Packet
                if (!BitUtils.ReadBit(1, data))
                    throw new CorruptedPacketException("Undefined packet header");
                // The two next bits describe the type of the Long Header Packet
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
                        throw new ArgumentException("2 Bit-encoded uint is not amongst {0, 1, 2, 3}");
                }
                p.Decode(data);
            }

            return p;
        }

        /// <summary>
        /// This method is called after the packet header has been decoded. It reads the payload and get
        /// the different frames out of it. 
        /// </summary>
        public virtual void DecodeFrames()
        {
            FrameParser fp = new FrameParser(Payload);
            if (Payload.Length == 0)
                return;

            Frames = fp.GetFrames();
            IsAckEliciting = fp.IsAckEliciting;
        }

        /// <summary>
        /// Add a frame to the payload (not encoded yet).
        /// </summary>
        /// <param name="frame">The frame to add to the packet</param>
        public virtual void AddFrame(Frame frame)
        {
            if (frame.Type == 0x01 || frame.Type > 0x03)
                IsAckEliciting = true;
            Frames.Add(frame);
        }

        /// <summary>
        /// Encode the frames previously added with AddFrame in the payload.
        /// </summary>
        /// <returns>The raw payload</returns>
        public virtual byte[] EncodeFrames()
        {
            List<byte> result = new List<byte>();
            foreach (Frame frame in Frames)
            {
                result.AddRange(frame.Encode());
            }

            return result.ToArray();
        }

        /// <summary>
        /// Decode the raw packet.
        /// </summary>
        /// <param name="data">The raw packet</param>
        /// <returns>Number of bits read</returns>
        public abstract int Decode(byte[] data);

        /// <summary>
        /// Encode the packet to a byte array.
        /// </summary>
        /// <returns>The raw packet</returns>
        public abstract byte[] Encode();
    }
}
