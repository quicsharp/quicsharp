using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp
{
    public static class BitUtils
    {
        public static void WriteBit(int index, byte[] data, bool b)
        {
            if (data.Length <= index / 8)
                throw new AccessViolationException($"QUIC packet too small (size: {data.Length * 8}, reading at: {index})");

            if (!b)
            {
                byte andNumber = Convert.ToByte(0xFF);
                andNumber -= Convert.ToByte(1 << (7 - (index % 8)));
                data[index / 8] = Convert.ToByte(data[index / 8] & andNumber);
            }
            else
            {
                byte orNumber = 0;
                orNumber += Convert.ToByte(1 << (7 - (index % 8)));
                data[index / 8] = Convert.ToByte(data[index / 8] | orNumber);
            }
        }
        public static void WriteNBits(int indexBegin, byte[] data, bool[] b)
        {
            if (data.Length <= (indexBegin / 8) + (b.Length / 8))
                throw new AccessViolationException($"QUIC packet too small (size: {data.Length * 8}, reading at: {indexBegin})");

            for (int i = 0; i < b.Length; i++)
            {
                WriteBit(indexBegin + i, data, b[i]);
            }
        }

        public static void WriteUInt32(int indexBegin, byte[] data, UInt32 toWrite)
        {
            if (data.Length < (indexBegin / 8) + 4)
                throw new AccessViolationException($"QUIC packet too small (size: {data.Length * 8}, reading at: {indexBegin})");

            for (int i = 0; i < 32; i++)
            {
                bool b = ((toWrite >> i) % 2 == 1);
                WriteBit(indexBegin + 31 - i, data, b);
            }
        }

        public static void WriteNByteFromInt(int indexBegin, byte[] data, uint toWrite, int n)
        {
            // TODO: use BitConverter instead https://docs.microsoft.com/en-us/dotnet/api/system.bitconverter
            if (data.Length < (indexBegin / 8) + n)
                throw new AccessViolationException($"QUIC packet too small (size: {data.Length * 8}, reading at: {indexBegin})");

            if (toWrite > Math.Pow(2, (8 * n)) - 1)
                throw new ArgumentException($"The following int can not be converted into {n} byte : {toWrite}");

            for (int i = 0; i < 8 * n; i++)
            {
                bool b = ((toWrite >> i) % 2 == 1);
                WriteBit(indexBegin + (8 * n) - 1 - i, data, b);
            }
        }

        public static bool ReadBit(int index, byte[] data)
        {
            if (data.Length <= index / 8)
                throw new AccessViolationException($"QUIC packet too small (size: {data.Length * 8}, reading at: {index})");

            // True if the bit n index is true
            return (data[index / 8] >> (7 - (index % 8))) % 2 == 1;
        }

        public static ulong LongReadNBits(int indexBegin, byte[] data, int n)
        {
            ulong ret = 0;

            if (data.Length < (indexBegin + n) / 8)
                throw new AccessViolationException($"QUIC packet too small (size: {data.Length * 8}, reading at: {indexBegin} + {n})");

            for (int i = 0; i < n; i++)
            {
                ret = ret << 1;
                if (ReadBit(indexBegin + i, data))
                {
                    ret += 1;
                }
            }

            return ret;
        }
        public static ulong LongReadNBits(int indexBegin, byte[] data, uint n)
        {
            return LongReadNBits(indexBegin, data, (int)n);
        }

        public static uint ReadNBits(int indexBegin, byte[] data, int n)
        {
            uint ret = 0;

            if (data.Length < (indexBegin + n) / 8)
                throw new AccessViolationException($"QUIC packet too small (size: {data.Length * 8}, reading at: {indexBegin} + {n})");

            for (int i = 0; i < n; i++)
            {
                ret = ret << 1;
                if (ReadBit(indexBegin + i, data))
                {
                    ret += 1;
                }
            }

            return ret;
        }

        public static uint ReadNBits(int indexBegin, byte[] data, uint n)
        {
            return ReadNBits(indexBegin, data, (int)n);
        }

        public static uint ReadByte(int indexBegin, byte[] data)
        {
            return ReadNBits(indexBegin, data, 8);
        }

        // TODO: n should be a ulong, not an int
        public static ulong ReadNBytes(int indexBegin, byte[] data, int n)
        {
            return LongReadNBits(indexBegin, data, n * 8);
        }

        public static ulong ReadNBytes(int indexBegin, byte[] data, uint n)
        {
            return ReadNBytes(indexBegin, data, (int)n);
        }

        public static uint ReadUInt32(int indexBegin, byte[] data)
        {
            uint ret = ReadNBits(indexBegin, data, 32);

            return ret;
        }
        public static ulong ReadUInt64(int indexBegin, byte[] data)
        {
            ulong ret = LongReadNBits(indexBegin, data, 64);

            return ret;
        }
    }
}
