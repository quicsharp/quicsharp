using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp
{
    /// <summary>
    /// Abstract class to define the different type of LongHeaderPackets (HandshakePacket, InitialPacket, RetryPacket, RTTPacket)
    /// Section 17.2
    /// </summary>
    public abstract class LongHeaderPacket : Packet
    {
        /* 
           +-+-+-+-+-+-+-+-+
           |1|1|T T|X X X X|
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
           |                         Version (32)                          |
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
           | DCID Len (8)  |
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
           |               Destination Connection ID (0..160)            ...     For our purposes, we decided that both DCID and SCID are encoded onto 4 bytes.
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
           | SCID Len (8)  |
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
           |                 Source Connection ID (0..160)               ...
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
           |           Payload (depending on the type of the packet)     ...
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
         */
        protected new static int packetHeaderSize_ = 10;
        protected static int maxCID_ = 20;

        public uint currentSupportedVersion => 0xff000017; // Only draft-23 is supported

        public uint PacketType;
        public uint Version;
        public uint DCIDLength;
        public byte[] DCID;
        public uint SCIDLength;
        public byte[] SCID;

        private uint headerSizeInBytes()
        {
            return 1 + 4 + 1 + DCIDLength + 1 + SCIDLength;
        }

        /// <summary>
        /// Decode the long header of the raw packet.
        /// </summary>
        /// <param name="data">The raw packet</param>
        /// <returns>Number of bits read</returns>
        public override int Decode(byte[] data)
        {
            if (data.Length < packetHeaderSize_)
                throw new CorruptedPacketException("QUIC packet too small for a LongHeaderPacket");

            // Read packet type
            int cursor = 2; // moving bit index
            PacketType = (uint)BitUtils.ReadNBits(cursor, data, 2);
            // Next 4 bits are packet specific and will be tended to in their own class.
            cursor += 6;

            // Read version
            Version = BitUtils.ReadUInt32(cursor, data);
            cursor += 32;
            if (Version != currentSupportedVersion)
                throw new NotImplementedException("Unsupported packet version");

            // Read DCID Len
            DCIDLength = BitUtils.ReadByte(cursor, data);
            if (DCIDLength > maxCID_)
                // Section 17.2: Endpoints that receive
                // a version 1 long header with a value larger than 20 MUST drop the
                // packet
                // TODO: skip this when functioning in server mode
                throw new CorruptedPacketException("DCID Len exceeded max value of 20");
            cursor += 8;

            // Read DCID
            DCID = new byte[DCIDLength];
            Array.Copy(data, cursor / 8, DCID, 0, DCIDLength);
            cursor += Convert.ToInt32(8 * DCIDLength);

            // Read SCID Len
            SCIDLength = BitUtils.ReadByte(cursor, data);
            if (SCIDLength > maxCID_)
                // Section 17.2: Endpoints that receive
                // a version 1 long header with a value larger than 20 MUST drop the
                // packet
                // TODO: skip this when functioning in server mode
                throw new CorruptedPacketException("SCID Len exceeded max value of 20");
            cursor += 8;

            // Read SCID
            SCID = new byte[SCIDLength];
            Array.Copy(data, cursor / 8, SCID, 0, SCIDLength);
            cursor += Convert.ToInt32(8 * SCIDLength);

            // TODO: read payload if applicable
            return cursor;
        }

        /// <summary>
        /// Encode the packet to a byte array. Encode the Header of the long header packets.
        /// </summary>
        /// <returns>The raw packet</returns>
        public override byte[] Encode()
        {
            byte[] packet = new byte[headerSizeInBytes()];
            BitUtils.WriteBit(0, packet, true);
            BitUtils.WriteBit(1, packet, true);
            int cursor = 8;

            Version = currentSupportedVersion;
            BitUtils.WriteUInt32(cursor, packet, Version);
            cursor += 32;

            BitUtils.WriteNByteFromInt(cursor, packet, (uint)DCIDLength, 1);
            cursor += 8;

            Array.Copy(DCID, 0, packet, cursor / 8, DCIDLength);
            // TODO: use a ulong for cursor
            cursor += 8 * (int)DCIDLength;

            BitUtils.WriteNByteFromInt(cursor, packet, (uint)SCIDLength, 1);
            cursor += 8;

            Array.Copy(SCID, 0, packet, cursor / 8, SCIDLength);
            // TODO: use a ulong for cursor
            cursor += 8 * (int)SCIDLength;

            // payload encoding is left to type-specific classes
            return packet;
        }
    }
}
