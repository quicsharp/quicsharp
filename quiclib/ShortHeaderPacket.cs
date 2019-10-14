﻿using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp
{
    // QUIC IETF draft 17.3
    public class ShortHeaderPacket : Packet
    {
        protected new static int packetHeaderSize_ = 9;
        public bool Spin = false;
        public bool KeyPhase = false;
        public int PacketNumberLengthByte = 4;

        public byte[] DestinationConnectionID = new byte[4];
        public uint PacketNumberLength;

        private int spinBit_ = 2;
        private int keyPhaseBit_ = 3;
        private int packetLengthBit_ = 6;
        private int destinationConnectionIDBit_ = 8;
        private int packetNumberBit_ = 40;
        public override int Decode(byte[] data)
        {
            if (data.Length < packetHeaderSize_)
                throw new AccessViolationException("QUIC packet too small for a ShortHeaderPacket");

            // TODO: Remove
            ClientId = 42;

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

            Spin = BitUtils.ReadBit(spinBit_, data);
            KeyPhase = BitUtils.ReadBit(keyPhaseBit_, data);
            // Reserved bits (R) are unused
            PacketNumberLength = BitUtils.ReadNBits(packetLengthBit_, data, 2) + 1;

            Array.Copy(data, destinationConnectionIDBit_ / 8, DestinationConnectionID, 0, 4);
            PacketNumber = (uint)BitUtils.ReadNBytes(packetNumberBit_, data, PacketNumberLength);

            Payload = new byte[data.Length - packetHeaderSize_ - PacketNumberLength];
            Array.Copy(data, packetHeaderSize_, Payload, 0, Payload.Length);

            // TODO: fix this
            return 0;
        }

        public override byte[] Encode()
        {
            Payload = EncodeFrames();
            byte[] packet = new byte[packetHeaderSize_ + Payload.Length];


            BitUtils.WriteBit(0, packet, false);

            BitUtils.WriteBit(1, packet, true);

            BitUtils.WriteBit(spinBit_, packet, Spin);
            BitUtils.WriteBit(keyPhaseBit_, packet, KeyPhase);

            BitUtils.WriteBit(packetLengthBit_, packet, ((PacketNumberLengthByte - 1) / 2) == 1);
            BitUtils.WriteBit(packetLengthBit_ + 1, packet, ((PacketNumberLengthByte - 1) % 2) == 1);

            Array.Copy(DestinationConnectionID, 0, packet, destinationConnectionIDBit_ / 8, 4);

            // TODO: Write N bits
            BitUtils.WriteUInt32(packetNumberBit_, packet, Convert.ToUInt32(PacketNumber));

            switch (PacketNumberLength - 1)
            {
                case 0:
                    BitUtils.WriteBit(6, packet, false);
                    BitUtils.WriteBit(7, packet, false);
                    BitUtils.WriteNByteFromInt(packetNumberBit_, packet, PacketNumber, 1);
                    break;
                case 1:
                    BitUtils.WriteBit(6, packet, false);
                    BitUtils.WriteBit(7, packet, true);
                    BitUtils.WriteNByteFromInt(packetNumberBit_, packet, PacketNumber, 2);
                    break;
                case 2:
                    BitUtils.WriteBit(6, packet, true);
                    BitUtils.WriteBit(7, packet, false);
                    BitUtils.WriteNByteFromInt(packetNumberBit_, packet, PacketNumber, 3);
                    break;
                case 3:
                    BitUtils.WriteBit(6, packet, true);
                    BitUtils.WriteBit(7, packet, true);
                    BitUtils.WriteNByteFromInt(packetNumberBit_, packet, PacketNumber, 4);
                    break;
            }

            Payload.CopyTo(packet, packetHeaderSize_);


            return packet;
        }
    }
}
