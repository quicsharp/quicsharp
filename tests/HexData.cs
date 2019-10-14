using System;

// Helper to manipulate sample packets from hex streams
namespace quicsharp.tests
{
    public class HexData
    {
        public string hexStream;

        public byte[] bytes
        {
            get
            {
                int NumberChars = hexStream.Length;
                byte[] bytes = new byte[NumberChars / 2];
                for (int i = 0; i < NumberChars; i += 2)
                    bytes[i / 2] = Convert.ToByte(hexStream.Substring(i, 2), 16);
                return bytes;
            }
        }

        public HexData(string hexStreamArg)
        {
            hexStream = hexStreamArg;
        }
    }
}
