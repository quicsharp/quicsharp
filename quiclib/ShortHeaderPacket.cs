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

        public uint DestinationConnectionID = 0;
        public ulong PacketNumber = 0;


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

            Spin = BitUtils.ReadBit(spinBit_, data);
            KeyPhase = BitUtils.ReadBit(keyPhaseBit_, data);
            // Reserved bits (R) are unused
            DecodePacketNumberLengthByte(data);

            DestinationConnectionID = BitUtils.ReadUInt32(destinationConnectionIDBit_, data);
            PacketNumber = BitUtils.LongReadNBits(packetNumberBit_, data, PacketNumberLengthByte * 8);

            Payload = new byte[data.Length - packetHeaderSize_];
            Array.Copy(data, packetHeaderSize_, Payload, 0, Payload.Length);
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

            BitUtils.WriteUInt32(destinationConnectionIDBit_, packet, DestinationConnectionID);

            // TODO: Write N bits
            BitUtils.WriteUInt32(packetNumberBit_, packet, Convert.ToUInt32(PacketNumber));

            Payload.CopyTo(packet, packetHeaderSize_);
               

            return packet;
        }

        private void DecodePacketNumberLengthByte(byte[] data)
        {
            PacketNumberLengthByte = (BitUtils.ReadBit(packetLengthBit_ + 1, data)) ? 1 : 0;
            PacketNumberLengthByte += (BitUtils.ReadBit(packetLengthBit_, data)) ? 2 : 0;

            PacketNumberLengthByte += 1;
        }
    }
}
