using System;
using System.Collections.Generic;
using System.Text;

using quicsharp.Frames;

namespace quicsharp
{
    class FrameParser
    {
        private byte[] content_;

        public FrameParser(byte[] content)
        {
            content_ = content;
        }

        public List<Frame> GetFrames()
        {
            if (content_.Length < 1)
                throw new ArgumentException("Corrupted frame");

            List<Frame> results = new List<Frame>();
            byte frameType = content_[0];
            int i = 0;

            while (i < content_.Length)
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
                    case 0x08: throw new NotImplementedException(); // StreamFrame(); break;
                    case 0x09: throw new NotImplementedException(); // StreamFrame(); break;
                    case 0x0a: throw new NotImplementedException(); // StreamFrame(); break;
                    case 0x0b: throw new NotImplementedException(); // StreamFrame(); break;
                    case 0x0c: throw new NotImplementedException(); // StreamFrame(); break;
                    case 0x0d: throw new NotImplementedException(); // StreamFrame(); break;
                    case 0x0e: throw new NotImplementedException(); // StreamFrame(); break;
                    case 0x0f: throw new NotImplementedException(); // StreamFrame(); break;
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

                if (results[results.Count - 1] != null)
                    i += results[results.Count - 1].Decode(content_, i);
            }

            return results;
        }
    }
}
