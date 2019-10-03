using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp
{
    class VariableLengthInteger
    {
        public int Size { get; private set; }

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
                    Size = 8;
                }
                else if (value >= (1 << 14))
                {
                    Size = 4;
                }
                else if (value >= (1 << 6))
                {
                    Size = 2;
                }
                else
                {
                    Size = 1;
                }
            }
        }

        VariableLengthInteger(UInt64 val)
        {
            Value = val;
        }

        public byte[] Encode()
        {
            byte[] encoded = new byte[Size];

            Packet.WriteBit(0, encoded, Size >= 4);
            Packet.WriteBit(1, encoded, Size == 2 || Size == 8);

            UInt64 v = value_;

            for (int i = encoded.Length - 1; i > 1; i--)
            {
                Packet.WriteBit(i, encoded, (v % 2) == 1);
                v = v >> 1;
            }

            return encoded;
        }

        public int Decode(int indexBegin, byte[] data)
        {
            // TODO: Check input
            Size = 0;
           
            switch (Packet.ReadNBits(indexBegin, data, 2))
            {
                case 0:
                    Size = 1;
                    value_ = (UInt64)Packet.ReadNBits(indexBegin + 2, data, 6);
                    break;
                case 1:
                    Size = 2;
                    value_ = (UInt64)Packet.ReadNBits(indexBegin + 2, data, 14);
                    break;
                case 2:
                    Size = 4;
                    value_ = (UInt64)Packet.ReadNBits(indexBegin + 2, data, 30);
                    break;
                case 3:
                    Size = 8;
                    value_ = (UInt64)Packet.ReadNBits(indexBegin + 2, data, 62);
                    break;
                default:
                    throw new Exception();
            }

            return Size * 8;
        }
    }
}
