using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp
{
    public class VariableLengthInteger
    {
        public int Size { get; private set; } // Number of bits

        private UInt64 value_;

        public UInt64 Value
        {
            get
            {
                return value_;
            }

            set
            {
                value_ = value;
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

        public byte[] Encode()
        {
            byte[] encoded = new byte[Size / 8];

            BitUtils.WriteBit(0, encoded, Size >= 32);
            BitUtils.WriteBit(1, encoded, Size == 16 || Size == 64);

            UInt64 v = value_;

            for (int i = (encoded.Length * 8) - 1; i > 1; i--)
            {
                BitUtils.WriteBit(i, encoded, (v % 2) == 1);
                v = v >> 1;
            }

            return encoded;
        }

        public int Decode(int indexBegin, byte[] data)
        {
            // TODO: Check input
            Size = 0;
           
            switch (BitUtils.ReadNBits(indexBegin, data, 2))
            {
                case 0:
                    Size = 8;
                    value_ = BitUtils.ReadNBits(indexBegin + 2, data, 6);
                    break;
                case 1:
                    Size = 16;
                    value_ = BitUtils.ReadNBits(indexBegin + 2, data, 14);
                    break;
                case 2:
                    Size = 32;
                    value_ = BitUtils.ReadNBits(indexBegin + 2, data, 30);
                    break;
                case 3:
                    Size = 64;
                    value_ = BitUtils.LongReadNBits(indexBegin + 2, data, 62);
                    break;
                default:
                    throw new Exception();
            }
            Console.WriteLine(value_);

            return Size;
        }
    }
}
