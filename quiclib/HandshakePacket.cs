using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp
{
    public sealed class HandshakePacket : LongHeaderPacket
    {
        public uint ReservedBits = 0;
        public uint PacketNumberLength;
        public VariableLengthInteger Length = new VariableLengthInteger(0);
        new public byte[] Payload;

        private static int reservedBitsIndex_ = 4;
        private static int packetNumberLengthBitsIndex_ = 6;
        private int packetNumberBitsIndex_;



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

        // TODO: remove once refactoring for variable length SCID and DCID is complete
        static int payloadStartBit_ = 120;

        public override int Decode(byte[] data)
        {
            int cursor = base.Decode(data);
            if (PacketType != 2)
                throw new ArgumentException("Wrong packet type");
            ReservedBits = BitUtils.ReadNBits(reservedBitsIndex_, data, 2);

            PacketNumberLength = BitUtils.ReadNBits(packetNumberLengthBitsIndex_, data, 2) + 1;
            if (PacketNumberLength >= 5 || PacketNumberLength == 0)
                throw new Exception("Invalid packet Number Length");

            cursor += Length.Decode(cursor, data);
            if ((UInt64)data.Length != Length.Value + (UInt64)cursor / 8)
                throw new CorruptedPacketException($"Handshake packet does not have the correct size. Expected: {Length.Value + (UInt64)cursor / 8} | Actual: {data.Length}");

            PacketNumber = (uint)BitUtils.ReadNBytes(cursor, data, PacketNumberLength);
            cursor += (Int32)PacketNumberLength * 8;

            Payload = new byte[data.Length - (cursor / 8)];
            Array.Copy(data, cursor / 8, Payload, 0, Payload.Length);
            cursor += 8 * Payload.Length;
            return cursor;
        }

        public override byte[] Encode()
        {
            List<byte> lpack = new List<byte>(base.Encode());

            Length.Value = (ulong)PacketNumberLength + (ulong)Payload.Length;
            lpack.AddRange(Length.Encode());

            packetNumberBitsIndex_ = lpack.Count * 8;

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
