using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp
{
    /// <summary>
    /// Abstract class to define a frame. The payload of a packet is filled with one or more frames.
    /// Different type of frame exist.
    /// </summary>
    public abstract class Frame
    {
        // Section 12.3 Table 3
        public abstract byte Type { get; }

        public abstract byte[] Encode();
        // Return the number of byte read
        public abstract int Decode(byte[] payload, int begin);
    }
}
