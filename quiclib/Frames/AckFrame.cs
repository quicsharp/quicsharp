using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp.Frames
{
    // Section 19.3
    class AckFrame : Frame
    {
        public override byte Type => 0x02;

        public UInt32 LargestAcknowledged;
        public UInt32 Delay;
        public UInt32 AckRangeCount;
        public UInt32 FirstAckRange;

        // Not doing AckRanges for now

        // ECN Counts 19.3.2
        public UInt32 ECT0;
        public UInt32 ECT1;
        public UInt32 ECN_CE;

        private int frameLengthBits => 8 + 7 * 32; 

        public override int Decode(byte[] content, int begin)
        {
            if (content.Length < frameLengthBits + begin)
                throw new ArgumentException("ACK Frame has a wrong size");
            if (content[begin] != Type)
                throw new ArgumentException("Wrong frame type created");

            int beginBits = begin * 8;

            LargestAcknowledged = Packet.ReadUInt32(beginBits + 8, content);
            Delay = Packet.ReadUInt32(beginBits + 40, content);
            AckRangeCount = Packet.ReadUInt32(beginBits + 72, content);
            FirstAckRange = Packet.ReadUInt32(beginBits + 104, content);

            // No AckRanges for now

            ECT0 = Packet.ReadUInt32(beginBits + 136, content);
            ECT1 = Packet.ReadUInt32(beginBits + 168, content);
            ECN_CE = Packet.ReadUInt32(beginBits + 200, content);

            return 29;
        }

        public override byte[] Encode()
        {
            byte[] content = new byte[frameLengthBits];

            Packet.WriteUInt32(8, content, LargestAcknowledged);
            Packet.WriteUInt32(40, content, Delay);
            Packet.WriteUInt32(72, content, AckRangeCount);
            Packet.WriteUInt32(108, content, FirstAckRange);

            // No AckRanges for now

            Packet.WriteUInt32(136, content, ECT0);
            Packet.WriteUInt32(168, content, ECT1);
            Packet.WriteUInt32(200, content, ECN_CE);

            return content;
        }
    }
}
