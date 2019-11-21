using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp.Frames
{
    /// <summary>
    /// Frame used for debugging. Not the specification.
    /// </summary>
    public class DebugFrame : Frame
    {
        public override byte Type => 0x1e;
        public string Message;

        /// <summary>
        /// Decode a DebugFrame from a raw byte array
        /// </summary>
        /// <param name="content">The raw byte array</param>
        /// <param name="begin">The bit index of the byte array where the AckFrame is located</param>
        /// <returns>The number of bits read</returns>
        public override int Decode(byte[] content, int begin)
        {
            if (content.Length < 1 + (begin / 8))
                throw new ArgumentException();
            if (content[begin] != Type)
                throw new ArgumentException($"Wrong frame type created got {content[begin]} instead of 0x1e (30)");

            List<byte> b = new List<byte>();
            for (int i = 1; i + begin < content.Length; i++)
            {
                b.Add(content[begin + i]);
            }
            Message = Encoding.Default.GetString(b.ToArray());

            return content.Length * 8;
        }

        /// <summary>
        /// Encode a DebugFrame to a raw byte array
        /// </summary>
        /// <returns>The encoded frame</returns>
        public override byte[] Encode()
        {
            List<byte> content = new List<byte>();
            content.Add(Type);

            content.AddRange(Encoding.ASCII.GetBytes(Message));

            return content.ToArray();
        }
    }
}
