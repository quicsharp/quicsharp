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

        public int PacketType;
        public UInt32 Version;
        public int DCIDLength;
        public UInt32 DCID;
        public int SCIDLength;
        public UInt32 SCID;
        public byte[] Payload;

        protected static int packetTypeBit_ = 2;
        protected static int versionBit_ = 8;
        protected static int DCIDLengthBit_ = 40;
        protected static int destinationConnectionIdBit_ = 48;
        protected static int SCIDLengthBit_ = 80;
        protected static int sourceConnectionIdBit_ = 88;
        protected static int payloadStartBit_ = 120;

        public override void Decode(byte[] data)
        {
            if (data.Length < packetHeaderSize_)
                throw new AccessViolationException("QUIC packet too small for a ShortHeaderPacket");

            PacketType = ReadNBits(packetTypeBit_, data, 2);
            // Next 4 bits are apcket specific and will be tended to in their own class.
            Version = ReadUInt32(versionBit_, data);

            DCIDLength = ReadByte(DCIDLengthBit_, data);
            if (DCIDLength != 4)
                throw new ArgumentException("In our implementation, we limit ourselves to 32 bits Destination connection IDs");
            DCID = ReadUInt32(destinationConnectionIdBit_, data);

            SCIDLength = ReadByte(SCIDLengthBit_, data);
            if (SCIDLength != 4)
                throw new ArgumentException(" In our implementation, we limit ourselves to 32 bits Destination connection IDs");
            else if(SCIDLength > 20)
            SCID = ReadUInt32(sourceConnectionIdBit_, data);

            Payload = new byte[data.Length - payloadStartBit_];
            Array.Copy(data, payloadStartBit_, Payload, 0, Payload.Length);
        }

        public override byte[] Encode()
        {
            byte[] packet = new byte[packetHeaderSize_ + Payload.Length];
            WriteBit(0, packet, true);
            WriteBit(1, packet, true);

            WriteUInt32(versionBit_, packet, Version);

            WriteNByteFromInt(DCIDLengthBit_, packet, (uint)DCIDLength, 1);
            WriteUInt32(destinationConnectionIdBit_, packet, DCID);


            WriteNByteFromInt(SCIDLengthBit_, packet, (uint)SCIDLength, 1);
            WriteUInt32(sourceConnectionIdBit_, packet, SCID);

            // payload encoding is left to type-speficic classes
            return packet;
        }
    }
}
