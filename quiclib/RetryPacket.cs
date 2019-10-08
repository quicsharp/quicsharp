using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp
{
    public sealed class RetryPacket : LongHeaderPacket
    {
        public byte[] RetryToken;
        public int ODCIDLength;
        public UInt32 ODCID;

        private static int ODCIDLengthBitsIndex_ = 120;
        private static int ODCIDBitsIndex_ = 128;
        private static int tokenBitsIndex_ = 160;




        /*
           +-+-+-+-+-+-+-+-+
           |1|1| 3 | Unused|
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
           | ODCID Len (8) |
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
           |          Original Destination Connection ID (0..160)        ...
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
           |                        Retry Token (*)                      ...
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
         */

        public override void Decode(byte[] data)
        {
            base.Decode(data);
            if (PacketType != 3)
                throw new ArgumentException("Wrong Packet type");

            ODCIDLength = ReadByte(ODCIDLengthBitsIndex_, data);
            if (ODCIDLength != 4)
                throw new ArgumentException("In our implementation, we limit ourselves to 32 bits Destination connection IDs");

            ODCID = ReadUInt32(ODCIDBitsIndex_, data);

            RetryToken = new byte[data.Length - (tokenBitsIndex_ / 8)];
            Array.Copy(data, tokenBitsIndex_ / 8, RetryToken, 0, RetryToken.Length);
        }

        public override byte[] Encode()
        {
            List<byte> lpack = new List<byte>(base.Encode());

            lpack.AddRange(new byte[5]); // OCDID length + value
            lpack.AddRange(RetryToken);
            byte[] packet = lpack.ToArray();

            WriteNByteFromInt(ODCIDLengthBitsIndex_, packet, (uint)ODCIDLength, 1);
            WriteUInt32(ODCIDBitsIndex_, packet, ODCID);

            WriteBit(2, packet, true);
            WriteBit(3, packet, true);
            return packet;
        }
    }
}
