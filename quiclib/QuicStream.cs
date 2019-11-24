using System;
using System.Collections.Generic;

using quicsharp.Frames;

namespace quicsharp
{
    /// <summary>
    /// Used to manage a specific stream between a two endpoints
    /// </summary>
    public class QuicStream
    {
        private QuicConnection connection_;
        private SortedList<UInt64, byte[]> _data = new SortedList<ulong, byte[]>();

        private VariableLengthInteger streamId_ = new VariableLengthInteger(0);
        public UInt64 StreamId
        {
            get
            {
                return streamId_.Value;
            }
            private set
            {
                streamId_.Value = value;
            }
        }
        public byte Type { get; private set; }

        /// <summary>
        /// Create a new stream between here and an endpoint
        /// </summary>
        /// <param name="connection">The other endpoint</param>
        /// <param name="streamId">The stream id</param>
        /// <param name="streamType">The type of the stream (unidirectional, bidirectional)</param>
        internal QuicStream(QuicConnection connection, VariableLengthInteger streamId, byte streamType)
        {
            StreamId = streamId.Value;

            connection_ = connection;
            Type = streamType;
        }

        /// <summary>
        /// Write a byte array in a stream. Then send this byte array
        /// </summary>
        /// <param name="buffer">The byte array to send</param>
        /// <param name="offset">The offset to consider in the byte array</param>
        /// <param name="size">Number of byte to send from the offset</param>
        public void Write(byte[] buffer, int offset, int size)
        {
            if (buffer == null || buffer.Length < offset + size)
                throw new ArgumentException();

            // TODO: check if the user is authorized to write (thanks to Type)
            byte[] data = new byte[size];
            Array.Copy(buffer, offset, data, 0, size);
            // TODO: may split the message on multiple frames
            StreamFrame frame = new StreamFrame(StreamId, 0, data, true, false);

            connection_.AddFrame(frame);
        }
    }
}
