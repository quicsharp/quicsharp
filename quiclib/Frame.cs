using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp
{
    public abstract class Frame
    {
        // Section 12.3 Table 3
        public abstract byte Type { get; }
        
        public byte[] Content;
        // False if the frame seems corrupted
        public bool Healthy = false;

        public abstract byte[] Encode();
        // Return the number of byte read
        public abstract int Decode(byte[] payload, int begin);
    }
}
