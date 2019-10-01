using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp
{
    class Frame
    {
        // Section 12.3 Table 3
        public enum FrameType {
            PADDING = 0x00,
            PING = 0x01,
            ACK1 = 0x02,
            ACK2 = 0x03,
            RESET_STREAM = 0x04,
            STOP_SENDING = 0x05,
            CRYPTO = 0x06,
            NEW_TOKEN = 0x07,
            STREAM_START = 0x08,
            STREAM_END = 0x0f,
            MAX_DATA = 0x10,
            MAX_STREAM_DATA = 0x11,
            MAX_STREAMS1 = 0x12,
            MAX_STREAMS2 = 0x13,
            DATA_BLOCKED = 0x14,
            STREAM_DATA_BLOCKED = 0x15,
            STREAMS_BLOCKED1 = 0x16,
            STREAMS_BLOCKED2 = 0x17,
            NEW_CONNECTION_ID = 0x18,
            RETIRE_CONNECTION_ID = 0x19,
            PATH_CHALLENGE = 0x1a,
            PATH_RESPONSE = 0x1b,
            CONNECTION_CLOSE1 = 0x1c,
            CONNECTION_CLOSE2 = 0x1d,
        };

        public FrameType Type;

        public byte[] Content;

        // False if the frame seems corrupted
        public bool Healthy = false;
    }
}
