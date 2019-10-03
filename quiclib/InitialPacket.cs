using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp
{
    sealed class InitialPacket : LongHeaderPacket
    {
        public int ReservedBits;
        public int PacketNumberLength;
        public UInt32 PacketNumber;
        public dynamic TokenLength;
        public UInt32 Token;
        public dynamic Length;
        public byte[] Payload;

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

        public override void Decode(byte[] data)
        {
            base.Decode(data);
            if (PacketType != 0)
                throw new ArgumentException("Wrong Packet type");
            ReservedBits = ReadNBits(reservedBitsIndex_, data, 2);

            PacketNumberLength = ReadNBits(packetNumberLengthBitsIndex_, data, 2) + 1;

            (tokenBitsIndex_, TokenLength) = ReadVariableLengthInteger(payloadStartBit_, data);
            Token = ReadNBytes(tokenBitsIndex_, data, TokenLength);


            // TODO : refactor once we made a class for variable-length integers + refactor bit reading/writing methods
            (packetNumberBitsIndex_, Length) = ReadVariableLengthInteger(tokenBitsIndex_ + (int)TokenLength, data);
            PacketNumber = (uint)ReadNBytes(packetNumberBitsIndex_, data, PacketNumberLength);

            Payload = new byte[data.Length - packetNumberBitsIndex_ - PacketNumberLength];
            Array.Copy(data, packetNumberBitsIndex_ + PacketNumberLength, Payload, 0, Payload.Length);
        }




    }
}
