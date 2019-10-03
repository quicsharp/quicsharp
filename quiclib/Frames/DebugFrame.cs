using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp.Frames
{
    // Frame used for debugging
    class DebugFrame : Frame
    {
        public override byte Type => 0x1e;
        public string Message;

        public override int Decode(byte[] content, int begin)
        {
            if (content.Length < 1 + begin)
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

        public override byte[] Encode()
        {
            List<byte> content = new List<byte>();
            content.Add(Type);

            content.AddRange(Encoding.ASCII.GetBytes(Message));

            return content.ToArray();
        }
    }
}
