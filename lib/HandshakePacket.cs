using System;
using System.Collections.Generic;

namespace quicsharp
{
    /// <summary>
    /// Packet used to carry acknowledgments and cryptographic handshake messages from the server and client
    /// Section 17.2.4
    /// </summary>
    public sealed class HandshakePacket : LongHeaderPacket
    {
        public uint PacketNumberLength;
        public VariableLengthInteger Length = new VariableLengthInteger(0);
        new public byte[] Payload;

        private uint _reservedBits = 0;
        private static int _reservedBitsIndex => 4;
        private static int _packetNumberLengthBitsIndex => 6;

        /*
           +-+-+-+-+-+-+-+-+
           |1|1| 2 |R R|P P|
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
           |                         Version (32)                          |
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
           | DCID Len (8)  |
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
           |               Destination Connection ID (0..160)            ...
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
           | SCID Len (8)  |
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
           |                 Source Connection ID (0..160)               ...
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
           |                           Length (i)                        ...
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
           |                    Packet Number (8/16/24/32)               ...
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
           |                          Payload (*)                        ...
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
         */

        /// <summary>
        /// Decode the raw packet to a HandshakePacket.
        /// </summary>
        /// <param name="data">The raw packet</param>
        /// <returns>Number of bits read</returns>
        public override int Decode(byte[] data)
        {
            int cursor = base.Decode(data);
            // Verify the PacketType
            if (PacketType != 2)
                throw new CorruptedPacketException("Wrong packet type");
            // Read the reserved bits (though they currently are not used)
            _reservedBits = BitUtils.ReadNBits(_reservedBitsIndex, data, 2);

            // Read and verify the range for PacketNumberLength (1 to 4)
            PacketNumberLength = BitUtils.ReadNBits(_packetNumberLengthBitsIndex, data, 2) + 1;
            if (PacketNumberLength >= 5 || PacketNumberLength == 0)
                throw new Exception("Invalid packet Number Length");

            // Verify that the packet length is the announced length
            cursor += Length.Decode(cursor, data);
            if ((UInt64)data.Length != Length.Value + (UInt64)cursor / 8)
                throw new CorruptedPacketException($"Handshake packet does not have the correct size. Expected: {Length.Value + (UInt64)cursor / 8} | Actual: {data.Length}");

            // Read the packet number
            PacketNumber = (uint)BitUtils.ReadNBytes(cursor, data, PacketNumberLength);
            cursor += (Int32)PacketNumberLength * 8;

            // Read the rest of the payload
            Payload = new byte[data.Length - (cursor / 8)];
            Array.Copy(data, cursor / 8, Payload, 0, Payload.Length);
            cursor += 8 * Payload.Length;
            return cursor;
        }

        /// <summary>
        /// Encode the HandshakePacket to a byte array. Encode the Header then the payload with all the frames.
        /// </summary>
        /// <returns>The raw packet</returns>
        public override byte[] Encode()
        {
            List<byte> lpack = new List<byte>(base.Encode());

            Length.Value = (ulong)PacketNumberLength + (ulong)Payload.Length;
            lpack.AddRange(Length.Encode());

            int packetNumberBitsIndex_ = lpack.Count * 8;

            lpack.AddRange(new byte[PacketNumberLength]);
            lpack.AddRange(Payload);

            // Set packet type to "Handshake Packet"
            lpack[0] &= 0b11001111; // clear
            lpack[0] += 0b00100000;

            // Set packet number length
            lpack[0] &= 0b11111100; // clear
            lpack[0] += Convert.ToByte(PacketNumberLength - 1);

            // Set packet number
            byte[] packet = lpack.ToArray();
            BitUtils.WriteNByteFromInt(packetNumberBitsIndex_, packet, PacketNumber, (int)PacketNumberLength);

            return packet;
        }
    }
}
