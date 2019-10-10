using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp
{
    class PacketManager
    {
        // Section 12.3
        private UInt32 packetNumber_ = 0;
        private UInt32 connectionID_ = 0;
        private UInt32 peerConnectionID_ = 0;

        public Queue<Packet> History = new Queue<Packet>();

        public PacketManager(UInt32 connectionID, UInt32 peerConnectionID)
        {
            connectionID_ = connectionID;
            peerConnectionID_ = peerConnectionID;
        }

        public ShortHeaderPacket CreateDataPacket(byte[] data)
        {
            ShortHeaderPacket packet = new ShortHeaderPacket {
                DestinationConnectionID = peerConnectionID_,
                PacketNumber = packetNumber_,
                PacketNumberLengthByte = 3,
                // TODO: frames
                Payload = data,
            };
            packetNumber_++;

            return packet;
        }

        public void Register(Packet p)
        {
            History.Enqueue(p);
        }
    }
}
