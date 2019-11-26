using System;
using System.Collections.Generic;

namespace quicsharp
{
    /// <summary>
    /// Used to carry "early" data from the client to the server as part of the first flight, prior to handshake completion. 
    /// As part of the TLS handshake, the server can accept or reject this early data.
    /// Section 17.2.3
    /// 
    /// Currently, since TLS is not implemented, the RTT packets are used to deliver StreamFrames.
    /// </summary>
    public sealed class RTTPacket : LongHeaderPacket
    {
        public uint PacketNumberLength = 4;
        public VariableLengthInteger Length = new VariableLengthInteger(0);

        private uint _reservedBits = 0;
        private static int _reservedBitsIndex => 4;
        private static int _packetNumberLengthBitsIndex => 6;

        /*
           +-+-+-+-+-+-+-+-+
           |1|1| 1 |R R|P P|
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

        public RTTPacket()
        {
        }

        public RTTPacket(byte[] dcid, byte[] scid, uint packetNumber)
        {
            PacketNumber = packetNumber;
            DCID = dcid;
            DCIDLength = (uint)DCID.Length;
            SCID = scid;
            SCIDLength = (uint)SCID.Length;
        }

        /// <summary>
        /// Decode the raw packet to a RTTPacket.
        /// </summary>
        /// <param name="data">The raw packet</param>
        /// <returns>Number of bits read</returns>
        public override int Decode(byte[] data)
        {
            // Decode the Long Header
            int cursor = base.Decode(data);
            if (PacketType != 1)
                throw new CorruptedPacketException("Wrong packet type");
            _reservedBits = BitUtils.ReadNBits(_reservedBitsIndex, data, 2);

            PacketNumberLength = BitUtils.ReadNBits(_packetNumberLengthBitsIndex, data, 2) + 1;
            if (PacketNumberLength >= 5 || PacketNumberLength == 0)
                throw new Exception("Invalid packet Number Length");

            cursor += Length.Decode(cursor, data);
            if ((UInt64)data.Length != Length.Value + (UInt64)cursor / 8)
                throw new CorruptedPacketException($"0-RTT packet does not have the correct size. Expected: {Length.Value + (UInt64)cursor / 8} | Actual: {data.Length}");

            PacketNumber = (uint)BitUtils.ReadNBytes(cursor, data, PacketNumberLength);
            cursor += (Int32)PacketNumberLength * 8;

            Payload = new byte[data.Length - (cursor / 8)];
            Array.Copy(data, cursor / 8, Payload, 0, Payload.Length);
            cursor += 8 * Payload.Length;
            return cursor;
        }

        /// <summary>
        /// Encode the RTTPacket to a byte array. Encode the Header then the payload with all the frames.
        /// </summary>
        /// <returns>The raw packet</returns>
        public override byte[] Encode()
        {
            List<byte> lpack = new List<byte>(base.Encode());
            Payload = EncodeFrames();

            Length.Value = (ulong)PacketNumberLength + (ulong)Payload.Length;
            lpack.AddRange(Length.Encode());

            int packetNumberBitsIndex_ = lpack.Count * 8;

            lpack.AddRange(new byte[PacketNumberLength]);
            lpack.AddRange(Payload);

            // Set packet type to "0-RTT Packet"
            lpack[0] &= 0b11001111; // clear
            lpack[0] += 0b00010000;

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