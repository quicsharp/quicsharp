using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;

using quicsharp.Frames;

namespace quicsharp
{
    public class PacketManager
    {
        // Section 12.3
        private UInt32 packetNumber_ = 0;
        public byte[] SCID = new byte[] { };
        public byte[] DCID = new byte[] { };

        public Dictionary<UInt32, Packet> History = new Dictionary<UInt32, Packet>();
        public List<UInt32> Received = new List<UInt32>();

        public PacketManager(byte[] scid, byte[] dcid)
        {
            SCID = scid;
            DCID = dcid;
        }

        public void PreparePacket(Packet packet)
        {
            if (packet is LongHeaderPacket)
            {
                LongHeaderPacket lhp = packet as LongHeaderPacket;
                lhp.DCID = DCID;
                lhp.DCIDLength = (UInt32)DCID.Length;
                lhp.SCID = SCID;
                lhp.SCIDLength = (UInt32)SCID.Length;
            }
            if (!(packet is RetryPacket))
            {
                packet.PacketNumber = ++packetNumber_;
                Register(packet, packet.PacketNumber);
            }
        }

        // Process a ack frame to remove packets that were acknowledged from the history
        public UInt32 ProcessAckFrame(AckFrame frame)
        {
            UInt32 ack = 0;
            UInt32 endOfRange = (UInt32)(frame.LargestAcknowledged.Value - frame.FirstAckRange.Value);

            for (UInt32 i = (UInt32)frame.LargestAcknowledged.Value; i > endOfRange; i--)
            {
                History.Remove(i);
            }

            foreach ((VariableLengthInteger, VariableLengthInteger) tuple in frame.AckRanges)
            {
                endOfRange -= (UInt32)tuple.Item1.Value;
                for (UInt32 j = 0; j < (UInt32)tuple.Item2.Value; j++)
                {
                    History.Remove(endOfRange);
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
