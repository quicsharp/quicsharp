﻿using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp
{
    public sealed class InitialPacket : LongHeaderPacket
    {
        public uint ReservedBits = 0;
        public uint PacketNumberLength;
        public uint PacketNumber;
        public VariableLengthInteger TokenLength = new VariableLengthInteger(0);
        public ulong Token;
        public VariableLengthInteger Length = new VariableLengthInteger(0);
        new public byte[] Payload;

        private static int reservedBitsIndex_ = 4;
        private static int packetNumberLengthBitsIndex_ = 6;
        private int tokenBitsIndex_;
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

        public override int Decode(byte[] data)
        {
            int cursor = base.Decode(data);
            if (PacketType != 0)
                throw new ArgumentException("Wrong Packet type");
            ReservedBits = BitUtils.ReadNBits(reservedBitsIndex_, data, 2);

            PacketNumberLength = BitUtils.ReadNBits(packetNumberLengthBitsIndex_, data, 2) + 1;
            if (PacketNumberLength > 5 || PacketNumberLength == 0)
                throw new Exception("Invalid Packet Number Length");

            cursor += TokenLength.Decode(cursor, data);

            // TODO: fix ReadNBytes so that it can take a ulong, then remove the conversion here
            Token = BitUtils.ReadNBytes(cursor, data, Convert.ToUInt32(TokenLength.Value));
            cursor += 8 * Convert.ToInt32(TokenLength.Value);

            cursor += Length.Decode(cursor, data);
            if ((UInt64)data.Length != Length.Value + (UInt64)cursor / 8)
                throw new CorruptedPacketException($"Initial Packet does not have the correct size. Expected: {Length.Value} | Actual: {data.Length}");

            PacketNumber = (uint)BitUtils.ReadNBytes(cursor, data, PacketNumberLength);
            cursor += (Int32)PacketNumberLength;

            Payload = new byte[data.Length - (cursor / 8) - PacketNumberLength];
            Array.Copy(data, cursor / 8, Payload, 0, Payload.Length);
            cursor += 8 * Payload.Length;
            return cursor;
        }

        public override byte[] Encode()
        {
            List<byte> lpack = new List<byte>(base.Encode());

            Payload = EncodeFrames();

            lpack.AddRange(TokenLength.Encode());

            tokenBitsIndex_ = lpack.Count * 8;
            lpack.AddRange(new byte[4]); // Token UInt32
            Length.Value = (ulong)lpack.Count + (ulong)Payload.Length + (ulong)PacketNumberLength + 1;
            Length.Value = Length.Value + (ulong)(Length.Size / 8);
            lpack.AddRange(Length.Encode());
            packetNumberBitsIndex_ = lpack.Count * 8;

            lpack.AddRange(new byte[PacketNumberLength + 1]); // Length + PacketNumber
            lpack.AddRange(Payload);
            byte[] packet = lpack.ToArray();
            BitUtils.WriteBit(2, packet, false);
            BitUtils.WriteBit(3, packet, false);

            BitUtils.WriteUInt32(tokenBitsIndex_, packet, (UInt32)Token);

            switch (PacketNumberLength - 1)
            {
                case 0:
                    BitUtils.WriteBit(6, packet, false);
                    BitUtils.WriteBit(7, packet, false);
                    BitUtils.WriteNByteFromInt(packetNumberBitsIndex_, packet, PacketNumber, 1);
                    break;
                case 1:
                    BitUtils.WriteBit(6, packet, false);
                    BitUtils.WriteBit(7, packet, true);
                    BitUtils.WriteNByteFromInt(packetNumberBitsIndex_, packet, PacketNumber, 2);
                    break;
                case 2:
                    BitUtils.WriteBit(6, packet, true);
                    BitUtils.WriteBit(7, packet, false);
                    BitUtils.WriteNByteFromInt(packetNumberBitsIndex_, packet, PacketNumber, 3);
                    break;
                case 3:
                    BitUtils.WriteBit(6, packet, true);
                    BitUtils.WriteBit(7, packet, true);
                    BitUtils.WriteNByteFromInt(packetNumberBitsIndex_, packet, PacketNumber, 4);
                    break;
            }

            return packet;
        }
    }
}
