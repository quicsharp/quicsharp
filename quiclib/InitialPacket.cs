using System;
using System.Collections.Generic;
using System.Linq;

using quicsharp.Frames;

namespace quicsharp
{
    /// <summary>
    /// Used to start a connection between a client and a server.
    /// Section 17.2.2
    /// </summary>
    public sealed class InitialPacket : LongHeaderPacket
    {
        public uint PacketNumberLength = 1;
        public VariableLengthInteger TokenLength = new VariableLengthInteger(0);
        public byte[] Token = new byte[0];
        public VariableLengthInteger Length = new VariableLengthInteger(0);

        private uint _reservedBits = 0;
        private static int _reservedBitsIndex => 4;
        private static int _packetNumberLengthBitsIndex => 6;

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
            DCIDLength = (uint)DCID.Length;
            SCID = scid;
            SCIDLength = (uint)SCID.Length;
        }

        /// <summary>
        /// Decode the raw packet.
        /// </summary>
        /// <param name="data">The raw packet</param>
        /// <returns>Number of bits read</returns>
        public override int Decode(byte[] data)
        {
            // Decode the Long Header
            int cursor = base.Decode(data);
            if (PacketType != 0)
                throw new CorruptedPacketException("Wrong packet type");
            _reservedBits = BitUtils.ReadNBits(_reservedBitsIndex, data, 2);

            PacketNumberLength = BitUtils.ReadNBits(_packetNumberLengthBitsIndex, data, 2) + 1;
            if (PacketNumberLength >= 5 || PacketNumberLength == 0)
                throw new Exception("Invalid packet Number Length");

            cursor += TokenLength.Decode(cursor, data);

            // TODO: fix ReadNBytes so that it can take a ulong, then remove the conversion here
            Token = new byte[TokenLength.Value];
            Array.Copy(data, cursor / 8, Token, 0, (uint)TokenLength.Value);
            cursor += 8 * Convert.ToInt32(TokenLength.Value);

            cursor += Length.Decode(cursor, data);
            if ((UInt64)data.Length != Length.Value + (UInt64)cursor / 8)
                throw new CorruptedPacketException($"Initial packet does not have the correct size. Expected: {Length.Value + (UInt64)cursor / 8} | Actual: {data.Length}");

            PacketNumber = (uint)BitUtils.ReadNBytes(cursor, data, PacketNumberLength);
            cursor += (Int32)PacketNumberLength * 8;

            Payload = new byte[data.Length - (cursor / 8)];
            Array.Copy(data, cursor / 8, Payload, 0, Payload.Length);
            cursor += 8 * Payload.Length;
            return cursor;
        }

        /// <summary>
        /// Encode the packet to a byte array. Encode the Header then the payload with all the frames.
        /// </summary>
        /// <returns>The raw packet</returns>
        public override byte[] Encode()
        {
            List<byte> lpack = new List<byte>(base.Encode());

            if (TokenLength.Value != (UInt64)Token.Length)
                throw new CorruptedPacketException("mismatch between Token.Length and TokenLength");

            lpack.AddRange(TokenLength.Encode());
            lpack.AddRange(Token);

            // Append padding frames so that the UDP datagram is at least 1200 bytes, per the spec
            // TODO: make this more efficient by computing the minimum number of padding frames needed
            byte[] padding = Enumerable.Repeat(new PaddingFrame().Type, 1200).ToArray();

            // TODO: use List<> everywhere instead of arrays?
            List<byte> PayloadList = new List<byte>(EncodeFrames());
            PayloadList.AddRange(padding);
            Payload = PayloadList.ToArray();

            Length.Value = (ulong)PacketNumberLength + (ulong)Payload.Length;
            lpack.AddRange(Length.Encode());

            int packetNumberBitsIndex_ = lpack.Count * 8;

            lpack.AddRange(new byte[PacketNumberLength]);
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
