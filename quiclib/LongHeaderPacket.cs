using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp
{
    // QUIC IETF draft 17.1
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
         */
        protected new static int packetHeaderSize_ = 15;
        protected static int maxCID_ = 20;

        public uint PacketType;
        public uint Version;
        public uint DCIDLength;
        public byte[] DCID;
        public uint SCIDLength;
        public byte[] SCID;
        new public byte[] Payload;

        // Start bit indexes of 
        protected static int packetTypeBit_ = 2;
        protected static int versionBit_ = 8;
        protected static int DCIDLengthBit_ = 40;
        protected static int destinationConnectionIdBit_ = 48;
        protected static int SCIDLengthBit_ = 80;
        protected static int sourceConnectionIdBit_ = 88;

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

        public override byte[] Encode()
        {
            byte[] packet = new byte[packetHeaderSize_];
            BitUtils.WriteBit(0, packet, true);
            BitUtils.WriteBit(1, packet, true);

            BitUtils.WriteUInt32(versionBit_, packet, Version);

            BitUtils.WriteNByteFromInt(DCIDLengthBit_, packet, (uint)DCIDLength, 1);
            BitUtils.WriteUInt32(destinationConnectionIdBit_, packet, DCID);


            BitUtils.WriteNByteFromInt(SCIDLengthBit_, packet, (uint)SCIDLength, 1);
            BitUtils.WriteUInt32(sourceConnectionIdBit_, packet, SCID);

            // payload encoding is left to type-speficic classes
            return packet;
        }
    }
}
