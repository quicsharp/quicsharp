using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp
{
    sealed class RetryPacket : LongHeaderPacket
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
                throw new ArgumentException(" In our implementation, we limit ourselves to 32 bits Destination connection IDs");
            else if (ODCIDLength > 20)
                ODCID = ReadUInt32(ODCIDBitsIndex_, data);

            RetryToken = new byte[data.Length - tokenBitsIndex_];
            Array.Copy(data, tokenBitsIndex_, Payload, 0, Payload.Length);

        }




    }
}
