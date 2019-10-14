using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp
{
    public class QuicStream
    {
        private QuicConnection connection_;
        private SortedList<UInt64, byte[]> _data = new SortedList<ulong, byte[]>();
        private UInt64 maximumStreamData;
        private UInt64 currentTransferRate;

        public VariableLengthInteger StreamId = new VariableLengthInteger(0);
        public byte Type;
        public QuicStream(QuicConnection connection, VariableLengthInteger streamId, byte streamType)
        {
            StreamId = streamId;

            maximumStreamData = 32; // TODO
            currentTransferRate = 0; // TODO
            connection_ = connection;
            Type = streamType;
        }
    }
}
