using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp.Frames
{
    class ResetStreamFrame : Frame
    {
        public override byte Type => 0x04;
        public VariableLengthInteger StreamID;
        public VariableLengthInteger ApplicationErrorCode;
        public VariableLengthInteger FinalSize;

        public override int Decode(byte[] content, int begin)
        {
            if (content.Length < 1 + (begin / 8))
                throw new ArgumentException();
            if (content[begin] != Type)
                throw new ArgumentException("Wrong frame type created");

            int beginBits = begin + 1;
            int read = 0;

            read += StreamID.Decode(beginBits + read, content);
            read += ApplicationErrorCode.Decode(begin + read, content);
            read += FinalSize.Decode(begin + read, content);

            return read;
        }

        public override byte[] Encode()
        {
            List<byte> content = new List<byte>();

            content.Add(Type);
            content.AddRange(StreamID.Encode());
            content.AddRange(ApplicationErrorCode.Encode());
            content.AddRange(FinalSize.Encode());

            return content.ToArray();
        }
    }
}
