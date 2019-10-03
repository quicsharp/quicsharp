using System;
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

        public UInt32 DestinationConnectionID = 0;
        public UInt64 PacketNumber = 0;


        private int spinBit_ = 2;
        private int keyPhaseBit_ = 3;
        private int packetLengthBit_ = 6;
        private int destinationConnectionIDBit_ = 8;
        private int packetNumberBit_ = 40;
        public override void Decode(byte[] data)
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

            Spin = Packet.ReadBit(spinBit_, data);
            KeyPhase = Packet.ReadBit(keyPhaseBit_, data);
            // Reserved bits (R) are unused
            DecodePacketNumberLengthByte(data);

            DestinationConnectionID = Packet.ReadUInt32(destinationConnectionIDBit_, data);
            PacketNumber = (UInt64)Packet.ReadNBits(packetNumberBit_, data, PacketNumberLengthByte * 8);

            Payload = new byte[data.Length - packetHeaderSize_];
            Array.Copy(data, packetHeaderSize_, Payload, 0, Payload.Length);
        }

        public override byte[] Encode()
        {
            Payload = EncodeFrames();
            byte[] packet = new byte[packetHeaderSize_ + Payload.Length];


            Packet.WriteBit(0, packet, false);

            Packet.WriteBit(1, packet, true);

            Packet.WriteBit(spinBit_, packet, Spin);
            Packet.WriteBit(keyPhaseBit_, packet, KeyPhase);

            Packet.WriteBit(packetLengthBit_, packet, ((PacketNumberLengthByte - 1) / 2) == 1);
            Packet.WriteBit(packetLengthBit_ + 1, packet, ((PacketNumberLengthByte - 1) % 2) == 1);

            Packet.WriteUInt32(destinationConnectionIDBit_, packet, DestinationConnectionID);

            // TODO: Write N bits
            Packet.WriteUInt32(packetNumberBit_, packet, Convert.ToUInt32(PacketNumber));

            Payload.CopyTo(packet, packetHeaderSize_);
               

            return packet;
        }

        private void DecodePacketNumberLengthByte(byte[] data)
        {
            PacketNumberLengthByte = (Packet.ReadBit(packetLengthBit_ + 1, data)) ? 1 : 0;
            PacketNumberLengthByte += (Packet.ReadBit(packetLengthBit_, data)) ? 2 : 0;

            PacketNumberLengthByte += 1;
        }
    }
}
