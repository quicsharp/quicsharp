using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp.Frames
{
    /// <summary>
    /// Frame used for padding inside the payload
    /// Section 19.1
    /// </summary>
    public class PaddingFrame : Frame
    {
        public override byte Type => 0x00;

        /// <summary>
        /// Decode a PaddingFrame from a raw byte array
        /// </summary>
        /// <param name="content">The raw byte array</param>
        /// <param name="begin">The bit index of the byte array where the AckFrame is located</param>
        /// <returns>The number of bits read</returns>
        public override int Decode(byte[] content, int begin)
        {
            if (content.Length < 1 + (begin / 8))
                throw new ArgumentException();
            if (content[begin / 8] != Type)
                throw new ArgumentException("Wrong frame type created");

            return 8;
        }

        /// <summary>
        /// Encode a PaddingFrame to a raw byte array
        /// </summary>
        /// <returns>The encoded frame</returns>
        public override byte[] Encode()
        {
            byte[] content = new byte[1];
            content[0] = Type;

            return content;
        }
    }
}
