using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp
{
    public sealed class InitialPacket : LongHeaderPacket
    {
        public uint ReservedBits = 0;
        public uint PacketNumberLength = 1;
        public VariableLengthInteger TokenLength = new VariableLengthInteger(0);
        public byte[] Token = new byte[0];
        public VariableLengthInteger Length = new VariableLengthInteger(0);

        private static int reservedBitsIndex_ = 4;
        private static int packetNumberLengthBitsIndex_ = 6;
        private int packetNumberBitsIndex_;

        /*
           +-+-+-+-+-+-+-+-+
           |1|1| 0 |R R|P P|
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
           |                         Token Length (i)                    ...
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
           |                            Token (*)                        ...
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
           |                           Length (i)                        ...
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
           |                    Packet Number (8/16/24/32)               ...
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
           |                          Payload (*)                        ...
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
         */
        public InitialPacket()
        {
        }

        public InitialPacket(byte[] dcid, byte[] scid, uint packetNumber)
        {
            PacketNumber = packetNumber;
            DCID = dcid;
            DCIDLength = (uint)dcid.Length;
            SCID = scid;
            SCIDLength = (uint)scid.Length;
        }

        public override int Decode(byte[] data)
        {
            int cursor = base.Decode(data);
            if (PacketType != 0)
                throw new ArgumentException("Wrong Packet type");
            ReservedBits = BitUtils.ReadNBits(reservedBitsIndex_, data, 2);

            PacketNumberLength = BitUtils.ReadNBits(packetNumberLengthBitsIndex_, data, 2) + 1;
            if (PacketNumberLength >= 5 || PacketNumberLength == 0)
                throw new Exception("Invalid Packet Number Length");

            cursor += TokenLength.Decode(cursor, data);

            // TODO: fix ReadNBytes so that it can take a ulong, then remove the conversion here
            Token = new byte[TokenLength.Value];
            Array.Copy(data, cursor / 8, Token, 0, (uint)TokenLength.Value);
            cursor += 8 * Convert.ToInt32(TokenLength.Value);

            cursor += Length.Decode(cursor, data);
            if ((UInt64)data.Length != Length.Value + (UInt64)cursor / 8)
                throw new CorruptedPacketException($"Initial Packet does not have the correct size. Expected: {Length.Value + (UInt64)cursor / 8} | Actual: {data.Length}");

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
            Payload = EncodeFrames();

            if (TokenLength.Value != (UInt64)Token.Length)
                throw new CorruptedPacketException("mismatch between Token.Length and TokenLength");

            lpack.AddRange(TokenLength.Encode());
            lpack.AddRange(Token);

            Length.Value = (ulong)PacketNumberLength + (ulong)Payload.Length;
            lpack.AddRange(Length.Encode());

            packetNumberBitsIndex_ = lpack.Count * 8;

            lpack.AddRange(new byte[PacketNumberLength]); // Length + PacketNumber
            lpack.AddRange(Payload);

            // Set packet type to "Initial Packet"
            lpack[0] &= 0b11001111;

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
