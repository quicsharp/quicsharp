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

        /// <summary>
        /// Abstract method to encode a specific frame to a raw byte array
        /// </summary>
        /// <returns>The encoded frame</returns>
        public abstract byte[] Encode();

        /// <summary>
        /// Abstract method to decode a specific from a raw byte array
        /// </summary>
        /// <param name="content">The raw byte array</param>
        /// <param name="begin">The bit index of the byte array where the AckFrame is located</param>
        /// <returns>The number of bits read</returns>
        public abstract int Decode(byte[] payload, int begin);
    }
}
