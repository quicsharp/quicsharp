﻿using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp.Frames
{
    /// <summary>
    /// Frame to handle the ack process
    /// Section 19.3
    /// </summary>
    public class AckFrame : Frame
    {
        public override byte Type => 0x02;

        public VariableLengthInteger LargestAcknowledged = new VariableLengthInteger(0);
        public VariableLengthInteger Delay = new VariableLengthInteger(0);
        public VariableLengthInteger AckRangeCount = new VariableLengthInteger(0);
        public VariableLengthInteger FirstAckRange = new VariableLengthInteger(0);
        public List<(VariableLengthInteger, VariableLengthInteger)> AckRanges = new List<(VariableLengthInteger, VariableLengthInteger)>();

        // ECN Counts 19.3.2
        public VariableLengthInteger ECT0 = new VariableLengthInteger(0);
        public VariableLengthInteger ECT1 = new VariableLengthInteger(0);
        public VariableLengthInteger ECN_CE = new VariableLengthInteger(0);

        private int frameLengthBitsMini => 8 + 7 * 8; 

        /*
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |                     Largest Acknowledged (i)                ...
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |                          ACK Delay (i)                      ...
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |                       ACK Range Count (i)                   ...
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |                       First ACK Range (i)                   ...
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |                          ACK Ranges (*)                     ...
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |                          [ECN Counts]                       ...
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        */

        /// <summary>
        /// Create a new ack frame from the lsit of the received packets.
        /// </summary>
        /// <param name="receivedPackets">The lsit of the received packets</param>
        /// <param name="delay">Delay of the ack frame (TODO: not used here)</param>
        public AckFrame(List<UInt32> receivedPackets, UInt64 delay)
        {
            if (receivedPackets.Count == 0)
                throw new ArgumentException("No packet to ack");
            receivedPackets.Sort();
            receivedPackets.Reverse();
            LargestAcknowledged.Value = receivedPackets[0];
            FirstAckRange.Value = 0;
            UInt32 ack = receivedPackets[0];
            UInt32 range = 1;

            Delay.Value = delay;

            for (int i = 1; i < receivedPackets.Count; i++)
            {
                if (ack - range == receivedPackets[i])
                    range++;
                else
                {
                    if (FirstAckRange.Value == 0)
                        FirstAckRange.Value = range;
                    else
                        AckRanges[AckRanges.Count - 1].Item1.Value = range;
                    range = 1;
                    AckRanges.Add((new VariableLengthInteger(0), new VariableLengthInteger(receivedPackets[i])));
                    ack = receivedPackets[i];
                }
            }

            if (FirstAckRange.Value == 0)
                FirstAckRange.Value = range;
            else
                AckRanges[AckRanges.Count - 1].Item1.Value = range;

            AckRangeCount.Value = (UInt64)AckRanges.Count;
        }

        public AckFrame()
        {

        }

        /// <summary>
        /// Decode an AckFrame from a raw byte array
        /// </summary>
        /// <param name="content">The raw byte array</param>
        /// <param name="begin">The bit index of the byte array where the AckFrame is located</param>
        /// <returns>The number of bits read</returns>
        public override int Decode(byte[] content, int begin)
        {
            if (content.Length < (frameLengthBitsMini + begin) / 8)
                throw new ArgumentException("ACK Frame has a wrong size");
            if (content[begin / 8] != Type)
                throw new ArgumentException("Wrong frame type created");

            int beginBits = begin;
            int read = 8;

            read += LargestAcknowledged.Decode(beginBits + read, content);
            read += Delay.Decode(beginBits + read, content);
            read += AckRangeCount.Decode(beginBits + read, content);
            read += FirstAckRange.Decode(beginBits + read, content);

            for (UInt32 i = 0; i < (UInt32)AckRangeCount.Value; i++)
            {
                AckRanges.Add((new VariableLengthInteger(0), new VariableLengthInteger(0)));
                read += AckRanges[AckRanges.Count - 1].Item1.Decode(beginBits + read, content);
                read += AckRanges[AckRanges.Count - 1].Item2.Decode(beginBits + read, content);
            }

            read += ECT0.Decode(beginBits + read, content);
            read += ECT1.Decode(beginBits + read, content);
            read += ECN_CE.Decode(beginBits + read, content);

            return read;
        }

        /// <summary>
        /// Encode an AckFrame to a raw byte array
        /// </summary>
        /// <returns>The encoded frame</returns>
        public override byte[] Encode()
        {
            List<byte> content = new List<byte>();

            content.Add(Type);

            AckRangeCount.Value = (UInt64)AckRanges.Count;

            content.AddRange(LargestAcknowledged.Encode());
            content.AddRange(Delay.Encode());
            content.AddRange(AckRangeCount.Encode());
            content.AddRange(FirstAckRange.Encode());
            
            foreach((VariableLengthInteger, VariableLengthInteger) t in AckRanges)
            {
                content.AddRange(t.Item1.Encode());
                content.AddRange(t.Item2.Encode());
            }

            content.AddRange(ECT0.Encode());
            content.AddRange(ECT1.Encode());
            content.AddRange(ECN_CE.Encode());

            return content.ToArray();
        }
    }
}
