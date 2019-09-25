﻿using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp
{
    // QUIC IETF draft 17.3
    class ShortHeaderPacket : Packet
    {
        protected new static int packetHeaderSize_ = 5;
        public bool Spin;
        public bool KeyPhase;
        public int PacketNumberLengthByte;

        // For our use case, this size is fixed to 4 bytes;
        public UInt32 DestinationConnectionID;
        public int PacketNumber;


        private int spinBit_ = 2;
        private int keyPhaseBit_ = 3;
        private int packetLengthBit_ = 6;
        private int packetNumberBit_ = 40;
        public override void Decode(byte[] data)
        {
            if (data.Length < packetHeaderSize_)
                throw new AccessViolationException("QUIC packet too small for a ShortHeaderPacket");

            // TODO: Remove
            ClientId = BitConverter.ToUInt32(data, 0);

            /* 
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
               | 0 | 1 | S | R | R | K | P P |
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
               | Destination Connection ID(0..160)           ...
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
               | Packet Number(8 / 16 / 24 / 32)...
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
               | Protected Payload(*)...
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
            */

            Spin = Packet.ReadBit(spinBit_, data);
            KeyPhase = Packet.ReadBit(keyPhaseBit_, data);
            // Reserved bits (R) are unused
            DecodePacketNumberLengthByte(data);

            DestinationConnectionID = Packet.ReadUInt32(8, data);
            PacketNumber = Packet.ReadNBits(packetNumberBit_, data, PacketNumberLengthByte * 8);

            Payload = new byte[data.Length - packetHeaderSize_ - PacketNumberLengthByte];
            Array.Copy(data, packetHeaderSize_ + PacketNumberLengthByte, Payload, 0, Payload.Length);
        }

        private void DecodePacketNumberLengthByte(byte[] data)
        {
            PacketNumberLengthByte = (Packet.ReadBit(packetLengthBit_ + 1, data)) ? 1 : 0;
            PacketNumberLengthByte += (Packet.ReadBit(packetLengthBit_, data)) ? 2 : 0;

            PacketNumberLengthByte += 1;
        }
    }
}
