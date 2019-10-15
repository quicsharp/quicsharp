using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp.Frames
{
    public class PingFrame : Frame
    {
        public override byte Type => 0x01;

        public override int Decode(byte[] content, int begin)
        {
            if (content.Length < 1 + (begin / 8))
                throw new ArgumentException();
            if (content[begin] != Type)
                throw new ArgumentException("Wrong frame type created");

            return 8;
        }

        public override byte[] Encode()
        {
            byte[] content = new byte[] { 1 };
            content[0] = Type;

            return content;
        }
    }
}
