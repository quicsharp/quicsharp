using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp
{
    public sealed class RetryPacket : LongHeaderPacket
    {
        public byte[] RetryToken;
        public uint ODCIDLength;
        public byte[] ODCID;

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

        public override int Decode(byte[] data)
        {
            int cursor = base.Decode(data);
            if (PacketType != 3)
                throw new ArgumentException("Wrong packet type");

            // Read ODCID Len
            ODCIDLength = BitUtils.ReadByte(cursor, data);
            if (ODCIDLength > maxCID_)
                // Section 17.2: Endpoints that receive
                // a version 1 long header with a value larger than 20 MUST drop the
                // packet
                // TODO: skip this when functioning in server mode
                throw new CorruptedPacketException("Original DCID Len exceeded max value of 20");
            cursor += 8;

            // Read ODCID
            ODCID = new byte[ODCIDLength];
            Array.Copy(data, cursor / 8, ODCID, 0, ODCIDLength);
            cursor += Convert.ToInt32(8 * ODCIDLength);

            RetryToken = new byte[data.Length - (cursor / 8)];
            Array.Copy(data, cursor / 8, RetryToken, 0, RetryToken.Length);
            cursor += 8 * RetryToken.Length;
            return cursor;
        }

        public override byte[] Encode()
        {
            List<byte> lpack = new List<byte>(base.Encode());

            lpack.Add(Convert.ToByte(ODCIDLength));
            lpack.AddRange(ODCID);
            lpack.AddRange(RetryToken);

            // Set packet type to "Retry Packet"
            lpack[0] &= 0b11001111; // clear
            lpack[0] += 0b00110000;

            return lpack.ToArray();
        }
    }
}
