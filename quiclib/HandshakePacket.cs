using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp
{
    sealed class HandshakePacket : LongHeaderPacket
    {
        public int ReservedBits;
        public int PacketNumberLength;
        public UInt32 PacketNumber;
        public dynamic Length;
        public byte[] Payload;

        private static int reservedBitsIndex_ = 4;
        private static int packetNumberLengthBitsIndex_ = 6;
        private int packetNumberBitsIndex_;



        /*
           +-+-+-+-+-+-+-+-+
           |1|1| 2 |R R|P P|
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
            if (PacketType != 2)
                throw new ArgumentException("Wrong Packet type");
            ReservedBits = ReadNBits(reservedBitsIndex_, data, 2);

            PacketNumberLength = ReadNBits(packetNumberLengthBitsIndex_, data, 2);

            // TODO : refactor once we made a class for variable-length integers + refactor bit reading/writing methods
            (packetNumberBitsIndex_, Length) = ReadVariableLengthInteger(payloadStartBit_, data);
            PacketNumber = (uint)ReadNBytes(packetNumberBitsIndex_, data, PacketNumberLength);

            Payload = new byte[data.Length - packetNumberBitsIndex_ - PacketNumberLength];
            Array.Copy(data, packetNumberBitsIndex_ + PacketNumberLength, Payload, 0, Payload.Length);
        }
    }
}
