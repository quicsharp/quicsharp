using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp.Frames
{
    /// <summary>
    /// Frame use to send message on a specific stream
    /// Section 19.8
    /// </summary>
    public class StreamFrame : Frame
    {
        // +---------------------+
        // | Frame type (8 bits) |
        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        // |                    Stream ID (i)                           ... 
        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        // |                    [Offset (i)]                            ... 
        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ 
        // |                    [Length (i)]                            ...
        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        // |                    Stream Data (*)                         ...
        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

        public override byte Type { get { return _writableType; } }
        public byte[] Data { get; private set; }

        // Frame type bits indicate the presence of the fields
        public bool OFF; // bit 0x04
        public bool LEN; // bit 0x02
        public bool FIN; // bit 0x01

        public VariableLengthInteger StreamID = new VariableLengthInteger(0);
        public VariableLengthInteger Offset = new VariableLengthInteger(0);
        private VariableLengthInteger _length = new VariableLengthInteger(0);

        private byte _writableType;
        private byte _minType => 0x08;
        private byte _maxType => 0x0f;

        public StreamFrame() { }

        public StreamFrame(UInt64 streamID, UInt64 offset, byte[] data, bool isLastFrameOfPacket, bool isEndOfStream)
        {
            StreamID = new VariableLengthInteger(streamID);

            OFF = offset != 0;
            Offset = new VariableLengthInteger(offset);

            LEN = !isLastFrameOfPacket;
            _length = new VariableLengthInteger(data.Length);

            FIN = isEndOfStream;
            _writableType = Convert.ToByte(0b00001000 + (OFF ? (1 << 2) : 0) + (LEN ? (1 << 1) : 0) + (FIN ? (1 << 0) : 0));

            Data = data;
        }

        /// <summary>
        /// Decode a StreamFrame from a raw byte array
        /// </summary>
        /// <param name="content">The raw byte array</param>
        /// <param name="begin">The bit index of the byte array where the AckFrame is located</param>
        /// <returns>The number of bits read</returns>
        public override int Decode(byte[] content, int begin)
        {
            if (content.Length < 1 + (begin / 8))
                throw new ArgumentException();


            _writableType = content[begin / 8];
            if (Type < _minType || Type > _maxType)
                throw new ArgumentException("Wrong frame type created");

            OFF = (Type & (1 << 2)) != 0;
            LEN = (Type & (1 << 1)) != 0;
            FIN = (Type & (1 << 0)) != 0;

            int cursor = (begin / 8) + 1;

            cursor += StreamID.Decode(cursor * 8, content) / 8;

            if (OFF)
            {
                cursor += Offset.Decode(cursor * 8, content) / 8;
            }

            if (LEN)
            {
                cursor += _length.Decode(cursor * 8, content) / 8;
            }
            else
            {
                _length = new VariableLengthInteger(Convert.ToUInt64(content.Length) - Convert.ToUInt64(cursor));
            }

            Data = new byte[_length.Value];

            // TODO: error handling if source packet is not long enough
            Array.Copy(content, cursor, Data, 0, Convert.ToInt32(_length.Value));
            return (cursor + Convert.ToInt32(_length.Value)) * 8 - begin;
        }

        /// <summary>
        /// Encode a StreamFrame to a raw byte array
        /// </summary>
        /// <returns>The encoded frame</returns>
        public override byte[] Encode()
        {
            List<byte> content = new List<byte>();
            content.Add(Type);
            content.AddRange(StreamID.Encode());
            if (OFF)
                content.AddRange(Offset.Encode());
            if (LEN)
                content.AddRange(_length.Encode());
            content.AddRange(Data);

            return content.ToArray();
        }
    }
}
