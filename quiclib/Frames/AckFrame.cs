using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp.Frames
{
    // Section 19.3
    class AckFrame : Frame
    {
        public override byte Type => 0x02;

        public VariableLengthInteger LargestAcknowledged;
        public VariableLengthInteger Delay;
        public VariableLengthInteger AckRangeCount;
        public VariableLengthInteger FirstAckRange;

        // Not doing AckRanges for now

        // ECN Counts 19.3.2
        public VariableLengthInteger ECT0;
        public VariableLengthInteger ECT1;
        public VariableLengthInteger ECN_CE;

        private int frameLengthBitsMini => 8 + 7 * 8; 

        public override int Decode(byte[] content, int begin)
        {
            if (content.Length < frameLengthBitsMini + begin)
                throw new ArgumentException("ACK Frame has a wrong size");
            if (content[begin] != Type)
                throw new ArgumentException("Wrong frame type created");

            int beginBits = (begin + 1) * 8;
            int read = 0;

            read += LargestAcknowledged.Decode(beginBits+ read, content);
            read += Delay.Decode(beginBits + read, content);
            read += AckRangeCount.Decode(beginBits + read, content);
            read += FirstAckRange.Decode(beginBits + read, content);

            // No AckRanges for now

            read += ECT0.Decode(beginBits + read, content);
            read += ECT1.Decode(beginBits + read, content);
            read += ECN_CE.Decode(beginBits + read, content);

            return (read / 8);
        }

        public override byte[] Encode()
        {
            List<byte> content = new List<byte>();

            content.AddRange(LargestAcknowledged.Encode());
            content.AddRange(Delay.Encode());
            content.AddRange(AckRangeCount.Encode());
            content.AddRange(FirstAckRange.Encode());

            // No AckRanges for now

            content.AddRange(ECT0.Encode());
            content.AddRange(ECT1.Encode());
            content.AddRange(ECN_CE.Encode());

            return content.ToArray();
        }
    }
}
