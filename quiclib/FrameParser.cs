using System;
using System.Collections.Generic;

using quicsharp.Frames;

namespace quicsharp
{
    /// <summary>
    /// FrameParser reads a payload and decode the frames inside it
    /// </summary>
    class FrameParser
    {
        /// <summary>
        /// The payload containing the frames
        /// </summary>
        private byte[] content_;

        /// <summary>
        /// An ack eliciting packet is a packet that contains at least one frame that is not a PingFrame or an AckFrame
        /// Section 13
        /// </summary>
        public bool IsAckEliciting = false;

        /// <summary>
        /// Creates the parser and initiate the payload to decode
        /// </summary>
        /// <param name="content">The content of the payload that contains the frames</param>
        public FrameParser(byte[] content)
        {
            content_ = new byte[content.Length];
            Array.Copy(content, content_, content.Length);
        }

        /// <summary>
        /// Decode the frames from the payload
        /// </summary>
        /// <returns>The list of the decoded frames</returns>
        public List<Frame> GetFrames()
        {
            if (content_.Length < 1)
                throw new ArgumentException("Corrupted frame");

            List<Frame> results = new List<Frame>();
            byte frameType = content_[0];
            int i = 0;

            while (i < content_.Length * 8)
            {
                switch (frameType)
                {
                    case 0x00: results.Add(new PaddingFrame()); break;
                    case 0x01: throw new NotImplementedException(); // PingFrame(); break;
                    case 0x02: results.Add(new AckFrame()); break;
                    case 0x03: results.Add(new AckFrame()); break;
                    case 0x04: throw new NotImplementedException(); // ResetStreamFrame(); break;
                    case 0x05: throw new NotImplementedException(); // StopSendingFrame(); break;
                    case 0x06: throw new NotImplementedException(); // CryptoFrame(); break;
                    case 0x07: throw new NotImplementedException(); // NewTokenFrame(); break;
                    case 0x08: results.Add(new StreamFrame()); break;
                    case 0x09: results.Add(new StreamFrame()); break;
                    case 0x0a: results.Add(new StreamFrame()); break;
                    case 0x0b: results.Add(new StreamFrame()); break;
                    case 0x0c: results.Add(new StreamFrame()); break;
                    case 0x0d: results.Add(new StreamFrame()); break;
                    case 0x0e: results.Add(new StreamFrame()); break;
                    case 0x0f: results.Add(new StreamFrame()); break;
                    case 0x10: throw new NotImplementedException(); // MaxDataFrame(); break;
                    case 0x11: throw new NotImplementedException(); // MaxStreamDataFrame(); break;
                    case 0x12: throw new NotImplementedException(); // MaxStreamsFrame(); break;
                    case 0x13: throw new NotImplementedException(); // MaxStreamsFrame(); break;
                    case 0x14: throw new NotImplementedException(); // DataBlockedFrame(); break;
                    case 0x15: throw new NotImplementedException(); // StreamDataBlockedFrame(); break;
                    case 0x16: throw new NotImplementedException(); // StreamsBlockedFrame(); break;
                    case 0x17: throw new NotImplementedException(); // StreamsBlockedFrame(); break;
                    case 0x18: throw new NotImplementedException(); // NewConnectionIdFrame(); break;
                    case 0x19: throw new NotImplementedException(); // RetireConnectionIdFrame(); break;
                    case 0x1a: throw new NotImplementedException(); // PathChallengeFrame(); break;
                    case 0x1b: throw new NotImplementedException(); // PathResponseFrame(); break;
                    case 0x1c: throw new NotImplementedException(); // ConnectionCloseFrame(); break;
                    case 0x1d: throw new NotImplementedException(); // ConnectionCloseFrame(); break;
                    case 0x1e: results.Add(new DebugFrame()); break;
                    default: results.Add(null); break;
                }

                if (frameType > 0x03 || frameType == 0x01)
                {
                    /* https://tools.ietf.org/html/draft-ietf-quic-recovery-23#section-2
                     * Ack-eliciting Frames:  All frames besides ACK or PADDING are
                          considered ack-eliciting.

                       Ack-eliciting Packets:  Packets that contain ack-eliciting frames
                          elicit an ACK from the receiver within the maximum ack delay and
                          are called ack-eliciting packets.
                     */

                    IsAckEliciting = true;
                }

                if (results[results.Count - 1] != null)
                {
                    i += results[results.Count - 1].Decode(content_, i);
                }
            }

            return results;
        }
    }
}
