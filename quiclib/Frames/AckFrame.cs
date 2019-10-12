using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp.Frames
{
    // Section 19.3
    public class AckFrame : Frame
    {
        public override byte Type => 0x02;

        public VariableLengthInteger LargestAcknowledged = new VariableLengthInteger(0);
        public VariableLengthInteger Delay = new VariableLengthInteger(0);
        public VariableLengthInteger AckRangeCount = new VariableLengthInteger(0);
        public VariableLengthInteger FirstAckRange = new VariableLengthInteger(0);
        public List<(VariableLengthInteger, VariableLengthInteger)> AckRanges = new List<(VariableLengthInteger, VariableLengthInteger)>();

        // ECN Counts 19.3.2
        public VariableLengthInteger ECT0 = new VariableLengthInteger(0);
        public VariableLengthInteger ECT1 = new VariableLengthInteger(0);
        public VariableLengthInteger ECN_CE = new VariableLengthInteger(0);

        private int frameLengthBitsMini => 8 + 7 * 8; 

        /*
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |                     Largest Acknowledged (i)                ...
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |                          ACK Delay (i)                      ...
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |                       ACK Range Count (i)                   ...
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |                       First ACK Range (i)                   ...
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |                          ACK Ranges (*)                     ...
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |                          [ECN Counts]                       ...
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        */

        public override int Decode(byte[] content, int begin)
        {
            if (content.Length < (frameLengthBitsMini + begin) / 8)
                throw new ArgumentException("ACK Frame has a wrong size");
            if (content[begin] != Type)
                throw new ArgumentException("Wrong frame type created");

            int beginBits = begin;
            int read = 8;

            read += LargestAcknowledged.Decode(beginBits + read, content);
            read += Delay.Decode(beginBits + read, content);
            read += AckRangeCount.Decode(beginBits + read, content);
            read += FirstAckRange.Decode(beginBits + read, content);

            for (UInt32 i = 0; i < (UInt32)AckRangeCount.Value; i++)
            {
                AckRanges.Add((new VariableLengthInteger(0), new VariableLengthInteger(0)));
                read += AckRanges[AckRanges.Count - 1].Item1.Decode(beginBits + read, content);
                read += AckRanges[AckRanges.Count - 1].Item2.Decode(beginBits + read, content);
            }        

            read += ECT0.Decode(beginBits + read, content);
            read += ECT1.Decode(beginBits + read, content);
            read += ECN_CE.Decode(beginBits + read, content);

            return read;
        }

        public override byte[] Encode()
        {
            List<byte> content = new List<byte>();

            content.Add(Type);

            AckRangeCount.Value = (UInt64)AckRanges.Count;

            content.AddRange(LargestAcknowledged.Encode());
            content.AddRange(Delay.Encode());
            content.AddRange(AckRangeCount.Encode());
            content.AddRange(FirstAckRange.Encode());
            
            foreach((VariableLengthInteger, VariableLengthInteger) t in AckRanges)
            {
                content.AddRange(t.Item1.Encode());
                content.AddRange(t.Item2.Encode());
            }

            content.AddRange(ECT0.Encode());
            content.AddRange(ECT1.Encode());
            content.AddRange(ECN_CE.Encode());

            return content.ToArray();
        }
    }
}
