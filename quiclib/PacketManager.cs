using System;
using System.Collections.Generic;
using System.Threading;

using quicsharp.Frames;

namespace quicsharp
{
    /// <summary>
    /// Used to manage the received packets and the sent packets.
    /// Store the packets that were sent but not ack as well as the packet numbers already received not to process them twice.
    /// </summary>
    public class PacketManager
    {
        // Section 12.3
        private UInt32 packetNumber_ = 0;

        // connID = connection ID = DCID of incoming packets, SCID of outgoing packets
        // peerID = DCID of outgoing packets, SCID of incoming packets
        public byte[] connID_ = new byte[] { };
        public byte[] peerID_ = new byte[] { };

        // Used to prevent race exception when sending packet again
        public Mutex HistoryMutex = new Mutex();
        public Dictionary<UInt32, Packet> History = new Dictionary<UInt32, Packet>();
        /// <summary>
        /// Received packets numbers. Used not to process the same packet twice. 
        /// </summary>
        private Dictionary<UInt32, bool> received_ = new Dictionary<UInt32, bool>();

        public PacketManager(byte[] connID, byte[] peerID)
        {
            connID_ = connID;
            peerID_ = peerID;
        }

        /// <summary>
        /// Register a packet and set its packet number, DCID and SCID to prepare it to be sent.
        /// </summary>
        /// <param name="packet">The packet to register</param>
        public void PreparePacket(Packet packet)
        {
            if (packet is LongHeaderPacket)
            {
                LongHeaderPacket lhp = packet as LongHeaderPacket;
                lhp.DCID_ = peerID_;
                lhp.DCIDLength_ = (UInt32)peerID_.Length;
                lhp.SCID_ = connID_;
                lhp.SCIDLength_ = (UInt32)connID_.Length;
            }
            // The retry packets do not have a packet number
            if (!(packet is RetryPacket))
            {
                packet.PacketNumber = ++packetNumber_;
                Register(packet, packet.PacketNumber);
            }
        }

        /// <summary>
        /// Process a ack frame to remove packets that were acknowledged from the history
        /// </summary>
        /// <param name="frame">The AckFrame to process</param>
        /// <returns>Number of packet ack</returns>
        public UInt32 ProcessAckFrame(AckFrame frame)
        {
            UInt32 ack = 0;
            UInt32 endOfRange = (UInt32)(frame.LargestAcknowledged.Value - frame.FirstAckRange.Value);
            HistoryMutex.WaitOne();

            for (UInt32 i = (UInt32)frame.LargestAcknowledged.Value; i > endOfRange; i--)
            {
                History.Remove(i);
                ack++;
            }

            foreach ((VariableLengthInteger, VariableLengthInteger) tuple in frame.AckRanges)
            {
                endOfRange -= (UInt32)tuple.Item1.Value;
                for (UInt32 j = 0; j < (UInt32)tuple.Item2.Value; j++)
                {
                    History.Remove(endOfRange);
                    endOfRange--;
                    ack++;
                }
            }
            HistoryMutex.ReleaseMutex();

            return ack;
        }

        /// <summary>
        /// Return true if the packet was already received at least once.
        /// </summary>
        public bool IsPacketOld(Packet packet)
        {
            if (packet is RetryPacket)
                return true;

            if (received_.ContainsKey(packet.PacketNumber))
                return true;

            received_.Add(packet.PacketNumber, true);
            return false;
        }

        /// <summary>
        /// Register the packet in history to be able to send it again if it was lost or corrupted.
        /// </summary>
        /// <param name="p">The packet to register</param>
        /// <param name="packetNumber">Its packet number</param>
        public void Register(Packet p, UInt32 packetNumber)
        {
            if (p.IsAckEliciting)
            {
                HistoryMutex.WaitOne();
                History.Add(packetNumber, p);
                HistoryMutex.ReleaseMutex();
            }
        }
    }
}
