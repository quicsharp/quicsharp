using System;

namespace quicsharp
{
    /// <summary>
    /// This is an unsigned integer as described in section 16.
    /// Is encoded on 8, 16, 32 or 64 bits depending on the 2 first bits.
    /// </summary>
    public class VariableLengthInteger
    {
        public int Size { get; private set; } // Number of bits

        private UInt64 _value;

        /// <summary>
        /// The value setter also modify the size of the integer.
        /// </summary>
        public UInt64 Value
        {
            get
            {
                return _value;
            }

            set
            {
                _value = value;
                if (value >= (1 << 30))
                {
                    Size = 64;
                }
                else if (value >= (1 << 14))
                {
                    Size = 32;
                }
                else if (value >= (1 << 6))
                {
                    Size = 16;
                }
                else
                {
                    Size = 8;
                }
            }
        }

        public VariableLengthInteger(UInt64 val = 0)
        {
            Value = val;
        }

        public VariableLengthInteger(int v = 0)
        {
            Value = (UInt64)v;
        }

        /// <summary>
        /// Encode the integer on the correct amount of bits.
        /// </summary>
        /// <returns>The encoded integer</returns>
        public byte[] Encode()
        {
            byte[] encoded = new byte[Size / 8];

            BitUtils.WriteBit(0, encoded, Size >= 32);
            BitUtils.WriteBit(1, encoded, Size == 16 || Size == 64);

            UInt64 v = _value;

            for (int i = (encoded.Length * 8) - 1; i > 1; i--)
            {
                BitUtils.WriteBit(i, encoded, (v % 2) == 1);
                v = v >> 1;
            }

            return encoded;
        }

        /// <summary>
        /// Decode a variable length integer in data starting at bit n° indexBegin
        /// </summary>
        /// <param name="indexBegin">Bit of the begining of the integer</param>
        /// <param name="data">Raw data</param>
        /// <returns>The number of bits read</returns>
        public int Decode(int indexBegin, byte[] data)
        {
            Size = 0;

            switch (BitUtils.ReadNBits(indexBegin, data, 2))
            {
                case 0:
                    Size = 8;
                    _value = BitUtils.ReadNBits(indexBegin + 2, data, 6);
                    break;
                case 1:
                    Size = 16;
                    _value = BitUtils.ReadNBits(indexBegin + 2, data, 14);
                    break;
                case 2:
                    Size = 32;
                    _value = BitUtils.ReadNBits(indexBegin + 2, data, 30);
                    break;
                case 3:
                    Size = 64;
                    _value = BitUtils.LongReadNBits(indexBegin + 2, data, 62);
                    break;
                default:
                    throw new Exception();
            }

            return Size;
        }
    }
}
