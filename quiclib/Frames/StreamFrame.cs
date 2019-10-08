using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp.Frames
{
    public class StreamFrame : Frame
    {
        // Section 19.8

        private byte minType => 0x08;
        private byte maxType => 0x0f;

        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        // |                    Stream ID (i)                           ... 
        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        // |                    [Offset (i)]                            ... 
        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ 
        // |                    [Length (i)]                            ...
        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        // |                    Stream Data (*)                         ...
        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        public VariableLengthInteger streamID;
        public VariableLengthInteger offset;
        public VariableLengthInteger length;
        public byte[] data;

        // Frame type bits indicate the presence of the fields
        public bool OFF; // bit 0x04
        public bool LEN; // bit 0x02
        public bool FIN; // bit 0x01

        public byte writableType;
        public override byte Type { get { return writableType; } }

        public override int Decode(byte[] content, int begin)
        {
            if (content.Length < 1 + begin)
                throw new ArgumentException();


            writableType = content[begin];
            if (Type < minType || Type > maxType)
                throw new ArgumentException("Wrong frame type created");

            OFF = (Type & (1 << 2)) != 0;
            LEN = (Type & (1 << 1)) != 0;
            FIN = (Type & (1 << 0)) != 0;

            int cursor = begin + 1;

            streamID = new VariableLengthInteger(0);
            cursor += streamID.Decode(cursor * 8, content) / 8;

            if (OFF)
            {
                offset = new VariableLengthInteger(0);
                cursor += offset.Decode(cursor * 8, content) / 8;
            }

            if (LEN)
            {
                length = new VariableLengthInteger(0);
                cursor += length.Decode(cursor * 8, content) / 8;
            }
            else
            {
                // TODO: harmonize all int types to UInt64
                length = new VariableLengthInteger(Convert.ToUInt64(content.Length) - Convert.ToUInt64(cursor));
            }

            // TODO(performance): immediately allocate the right amount of memory for 'data'
            data = new byte[length.Value];

            // TODO: error handling if source packet is not long enough
            Array.Copy(content, cursor, data, 0, Convert.ToInt32(length.Value));
            return (cursor + Convert.ToInt32(length.Value) - begin) / 8;
        }

        public override byte[] Encode()
        {
            byte[] content = new byte[1];
            content[0] = Type;

            return content;
        }
    }
}
