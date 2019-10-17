using System;
using System.Collections.Generic;

using quicsharp.Frames;

namespace quicsharp
{
    public class QuicStream
    {
        private QuicConnection connection_;
        private SortedList<UInt64, byte[]> _data = new SortedList<ulong, byte[]>();
        private UInt64 maximumStreamData;
        private UInt64 currentTransferRate;

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
        internal QuicStream(QuicConnection connection, VariableLengthInteger streamId, byte streamType)
        {
            StreamId = streamId.Value;

            maximumStreamData = 32; // TODO
            currentTransferRate = 0; // TODO
            connection_ = connection;
            Type = streamType;
        }

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
