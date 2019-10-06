using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp
{
    public sealed class InitialPacket : LongHeaderPacket
    {
        public int ReservedBits = 0;
        public int PacketNumberLength;
        public UInt32 PacketNumber;
        public VariableLengthInteger TokenLength;
        public UInt32 Token;
        public VariableLengthInteger Length;
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

            TokenLength.Decode(payloadStartBit_, data);
            tokenBitsIndex_ = payloadStartBit_ + TokenLength.Size * 8;

            Token = ReadUInt32(tokenBitsIndex_, data);


            Length.Decode(tokenBitsIndex_ + 32, data);
            packetNumberBitsIndex_ = tokenBitsIndex_ + 32 + Length.Size * 8;
            PacketNumber = (uint)ReadNBytes(packetNumberBitsIndex_, data, PacketNumberLength * 8);

            Payload = new byte[data.Length - packetNumberBitsIndex_ - PacketNumberLength * 8];
            Array.Copy(data, packetNumberBitsIndex_ + PacketNumberLength * 8, Payload, 0, Payload.Length);
        }

        public override byte[] Encode()
        {
            byte[] packet = base.Encode();
            WriteBit(2, packet, false);
            WriteBit(3, packet, false);
            
            Array.Copy(TokenLength.Encode(), 0, packet, payloadStartBit_, TokenLength.Size * 8);
            WriteUInt32(tokenBitsIndex_, packet, Token);
            Array.Copy(Length.Encode(), 0, packet, tokenBitsIndex_ + 32, Length.Size * 8);

            switch (PacketNumberLength)
            {
                case 0:
                    WriteBit(6, packet, false);
                    WriteBit(7, packet, false);
                    WriteNByteFromInt(packetNumberBitsIndex_, packet, PacketNumber, 1);
                    break;
                case 1:
                    WriteBit(6, packet, false);
                    WriteBit(7, packet, true);
                    WriteNByteFromInt(packetNumberBitsIndex_, packet, PacketNumber, 2);
                    break;
                case 2:
                    WriteBit(6, packet, true);
                    WriteBit(7, packet, false);
                    WriteNByteFromInt(packetNumberBitsIndex_, packet, PacketNumber, 3);
                    break;
                case 3:
                    WriteBit(6, packet, true);
                    WriteBit(7, packet, true);
                    WriteNByteFromInt(packetNumberBitsIndex_, packet, PacketNumber, 4);
                    break;
            }

            Array.Copy(Payload, 0, packet, packetNumberBitsIndex_ + PacketNumberLength * 8, Payload.Length);
            return packet;
        }
    }
}
