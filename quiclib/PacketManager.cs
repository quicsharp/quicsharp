using System;
using System.Collections.Generic;
using System.Text;

using quicsharp.Frames;

namespace quicsharp
{
    class PacketManager
    {
        // Section 12.3
        private UInt32 packetNumber_ = 0;
        private UInt32 connectionID_ = 0;
        private UInt32 peerConnectionID_ = 0;

        public Dictionary<UInt32, Packet> History = new Dictionary<UInt32, Packet>();

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

        public UInt32 ProcessAckFrame(AckFrame frame)
        {
            UInt32 ack = 0;
            UInt32 endOfRange =  (UInt32)(frame.LargestAcknowledged.Value - frame.FirstAckRange.Value);

            for (UInt32 i = (UInt32)frame.LargestAcknowledged.Value; i >= endOfRange; i--)
            {
                History.Remove(i);
            }

            for (UInt32 i = 0; i < frame.AckRangeCount.Value; i++)
            {
                endOfRange -= (UInt32)frame.AckRanges[i].Item1.Value;
                for (UInt32 j = 0; j < frame.AckRanges[i].Item2.Value; j++)
                {
                    History.Remove(i);
                    endOfRange--;
                }
            }

            return ack;
        }

        public void Register(Packet p, UInt32 packetNumber)
        {
            History.Add(packetNumber, p);
        }
    }
}
