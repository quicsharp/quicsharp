using System;
using System.Collections.Generic;
using System.Threading;
using quicsharp.Frames;

namespace quicsharp
{
    /// <summary>
    /// Used to manage a specific stream between a two endpoints
    /// </summary>
    public class QuicStream
    {
        private QuicConnection _connection;
        private Queue<StreamFrame> _toRead = new Queue<StreamFrame>();
        private VariableLengthInteger _streamId = new VariableLengthInteger(0);
        private ManualResetEvent _mre = new ManualResetEvent(false);

        public UInt64 StreamId
        {
            get
            {
                return _streamId.Value;
            }
            private set
            {
                _streamId.Value = value;
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

            _connection = connection;
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

            _connection.AddFrame(frame);
            _connection.SendCurrentPacket();
        }

        /// <summary>
        /// Add a stream frame to the queue of frames to read
        /// </summary>
        /// <param name="sf">StreamFrame to be added to the queue</param>
        public void AddFrameToRead(StreamFrame sf)
        {
            _toRead.Enqueue(sf);
            _mre.Set();
        }

        /// <summary>
        /// Return the byte array corresponding to the data from the next frame to be read
        /// </summary>
        /// <returns></returns>
        public byte[] Read()
        {
            _mre.WaitOne();
            StreamFrame frame = _toRead.Dequeue();
            if (_toRead.Count == 0)
            {
                _mre.Reset();
            }
            return frame.Data;
        }
    }
}