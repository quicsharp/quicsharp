using System;

namespace quicsharp
{
    // QUIC IETF draft 17.3
    public class ShortHeaderPacket : Packet
    {
        public bool Spin = false;
        public bool KeyPhase = false;

        // Only 4 bytes DCID is used for now
        // TODO: extend this to arbitrary number of bytes
        public byte[] DCID = new byte[4];
        public uint PacketNumberLength = 4;

        private static int _packetHeaderSize => 9;
        private int _spinBit = 2;
        private int _keyPhaseBit = 3;
        private int _packetLengthBit = 6;
        private int _destinationConnectionIDBit = 8;
        private int _packetNumberBit = 40;

        /// <summary>
        /// Decode the raw packet.
        /// </summary>
        /// <param name="data">The raw packet</param>
        /// <returns>Number of bits read</returns>
        public override int Decode(byte[] data)
        {
            if (data.Length < _packetHeaderSize)
                throw new CorruptedPacketException("QUIC packet too small for a ShortHeaderPacket");

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

            Spin = BitUtils.ReadBit(_spinBit, data);
            KeyPhase = BitUtils.ReadBit(_keyPhaseBit, data);
            // Reserved bits (R) are unused
            PacketNumberLength = BitUtils.ReadNBits(_packetLengthBit, data, 2) + 1;

            Array.Copy(data, _destinationConnectionIDBit / 8, DCID, 0, 4);
            PacketNumber = (uint)BitUtils.ReadNBytes(_packetNumberBit, data, PacketNumberLength);

            Payload = new byte[data.Length - _packetHeaderSize];
            Array.Copy(data, _packetHeaderSize, Payload, 0, Payload.Length);

            // TODO: fix this
            return 0;
        }

        /// <summary>
        /// Encode the packet to a byte array. Encode the Header then the payload with all the frames.
        /// </summary>
        /// <returns>The raw packet</returns>
        public override byte[] Encode()
        {
            Payload = EncodeFrames();
            byte[] packet = new byte[_packetHeaderSize + Payload.Length];


            BitUtils.WriteBit(0, packet, false);

            BitUtils.WriteBit(1, packet, true);

            BitUtils.WriteBit(_spinBit, packet, Spin);
            BitUtils.WriteBit(_keyPhaseBit, packet, KeyPhase);

            BitUtils.WriteBit(_packetLengthBit, packet, ((PacketNumberLength - 1) / 2) == 1);
            BitUtils.WriteBit(_packetLengthBit + 1, packet, ((PacketNumberLength - 1) % 2) == 1);

            Array.Copy(DCID, 0, packet, _destinationConnectionIDBit / 8, 4);

            // TODO: Write N bits
            BitUtils.WriteUInt32(_packetNumberBit, packet, Convert.ToUInt32(PacketNumber));

            // The 2 first bits for the packet number length
            BitUtils.WriteNBits(_packetLengthBit, packet, new bool[] { PacketNumberLength > 2, PacketNumberLength % 2 == 0 });
            // The other for the packet number itself
            BitUtils.WriteNByteFromInt(_packetNumberBit, packet, PacketNumber, (int)PacketNumberLength);

            Payload.CopyTo(packet, _packetHeaderSize);


            return packet;
        }
    }
}
