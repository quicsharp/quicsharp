using System;

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
           |                         version_ (32)                          |
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

        public uint currentSupportedVersion_ => 0xff000017; // Only draft-23 is supported

        public uint packetType_;
        public uint version_;
        public uint DCIDLength_;
        public byte[] DCID_;
        public uint SCIDLength_;
        public byte[] SCID_;

        private uint headerSizeInBytes()
        {
            return 1 + 4 + 1 + DCIDLength_ + 1 + SCIDLength_;
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
            packetType_ = (uint)BitUtils.ReadNBits(cursor, data, 2);
            // Next 4 bits are packet specific and will be tended to in their own class.
            cursor += 6;

            // Read version
            version_ = BitUtils.ReadUInt32(cursor, data);
            cursor += 32;
            if (version_ != currentSupportedVersion_)
                throw new NotImplementedException("Unsupported packet version_");

            // Read DCID Len
            DCIDLength_ = BitUtils.ReadByte(cursor, data);
            if (DCIDLength_ > maxCID_)
                // Section 17.2: Endpoints that receive
                // a version_ 1 long header with a value larger than 20 MUST drop the
                // packet
                // TODO: skip this when functioning in server mode
                throw new CorruptedPacketException("DCID Len exceeded max value of 20");
            cursor += 8;

            // Read DCID
            DCID_ = new byte[DCIDLength_];
            Array.Copy(data, cursor / 8, DCID_, 0, DCIDLength_);
            cursor += Convert.ToInt32(8 * DCIDLength_);

            // Read SCID Len
            SCIDLength_ = BitUtils.ReadByte(cursor, data);
            if (SCIDLength_ > maxCID_)
                // Section 17.2: Endpoints that receive
                // a version_ 1 long header with a value larger than 20 MUST drop the
                // packet
                // TODO: skip this when functioning in server mode
                throw new CorruptedPacketException("SCID Len exceeded max value of 20");
            cursor += 8;

            // Read SCID
            SCID_ = new byte[SCIDLength_];
            Array.Copy(data, cursor / 8, SCID_, 0, SCIDLength_);
            cursor += Convert.ToInt32(8 * SCIDLength_);

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

            version_ = currentSupportedVersion_;
            BitUtils.WriteUInt32(cursor, packet, version_);
            cursor += 32;

            BitUtils.WriteNByteFromInt(cursor, packet, (uint)DCIDLength_, 1);
            cursor += 8;

            Array.Copy(DCID_, 0, packet, cursor / 8, DCIDLength_);
            // TODO: use a ulong for cursor
            cursor += 8 * (int)DCIDLength_;

            BitUtils.WriteNByteFromInt(cursor, packet, (uint)SCIDLength_, 1);
            cursor += 8;

            Array.Copy(SCID_, 0, packet, cursor / 8, SCIDLength_);
            // TODO: use a ulong for cursor
            cursor += 8 * (int)SCIDLength_;

            // payload encoding is left to type-specific classes
            return packet;
        }
    }
}
