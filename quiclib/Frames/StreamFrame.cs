using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp.Frames
{
    public class StreamFrame : Frame
    {
        // Section 19.8

        private byte _minType => 0x08;
        private byte _maxType => 0x0f;

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
        public VariableLengthInteger _streamID = new VariableLengthInteger(0);
        public VariableLengthInteger _offset = new VariableLengthInteger(0);
        public VariableLengthInteger _length = new VariableLengthInteger(0);
        public byte[] Data { get; private set; }

        // Frame type bits indicate the presence of the fields
        public bool _OFF; // bit 0x04
        public bool _LEN; // bit 0x02
        public bool _FIN; // bit 0x01

        public byte _writableType;
        public override byte Type { get { return _writableType; } }

        public StreamFrame() { }

        public StreamFrame(UInt64 streamID, UInt64 offset, byte[] data, bool isLastFrameOfPacket, bool isEndOfStream)
        {
            _streamID = new VariableLengthInteger(streamID);

            _OFF = offset != 0;
            _offset = new VariableLengthInteger(offset);

            _LEN = !isLastFrameOfPacket;
            _length = new VariableLengthInteger(data.Length);

            _FIN = isEndOfStream;
            _writableType = Convert.ToByte(0b00001000 + (_OFF ? (1 << 2) : 0) + (_LEN ? (1 << 1) : 0) + (_FIN ? (1 << 0) : 0));

            Data = data;
        }

        public override int Decode(byte[] content, int begin)
        {
            if (content.Length < 1 + begin)
                throw new ArgumentException();


            _writableType = content[begin];
            if (Type < _minType || Type > _maxType)
                throw new ArgumentException("Wrong frame type created");

            _OFF = (Type & (1 << 2)) != 0;
            _LEN = (Type & (1 << 1)) != 0;
            _FIN = (Type & (1 << 0)) != 0;

            int cursor = begin + 1;

            cursor += _streamID.Decode(cursor * 8, content) / 8;

            if (_OFF)
            {
                cursor += _offset.Decode(cursor * 8, content) / 8;
            }

            if (_LEN)
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
            return (cursor + Convert.ToInt32(_length.Value) - begin) * 8;
        }

        public override byte[] Encode()
        {
            List<byte> content = new List<byte>();
            content.Add(Type);
            content.AddRange(_streamID.Encode());
            if (_OFF)
                content.AddRange(_offset.Encode());
            if (_LEN)
                content.AddRange(_length.Encode());
            content.AddRange(Data);

            return content.ToArray();
        }
    }
}
