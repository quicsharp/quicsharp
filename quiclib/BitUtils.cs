using System;
using System.Collections.Generic;
using System.Text;

namespace quicsharp
{
    public static class BitUtils
    {
        /// <summary>
        /// Write the bit b at the bit n°index in data
        /// </summary>
        /// <param name="index">The bit location</param>
        /// <param name="data">The full raw data</param>
        /// <param name="b">The bit to write</param>
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

        /// <summary>
        /// Write the bits b in data starting at bit n°indexBegin 
        /// </summary>
        /// <param name="indexBegin">Starting bit location</param>
        /// <param name="data">The full raw data</param>
        /// <param name="b">The bits to write</param>
        public static void WriteNBits(int indexBegin, byte[] data, bool[] b)
        {
            if (data.Length <= (indexBegin / 8) + (b.Length / 8))
                throw new AccessViolationException($"QUIC packet too small (size: {data.Length * 8}, reading at: {indexBegin})");

            for (int i = 0; i < b.Length; i++)
            {
                WriteBit(indexBegin + i, data, b[i]);
            }
        }

        /// <summary>
        /// Write an unigned integer on 32 bits to data, starting at bit n°indexBegin
        /// </summary>
        /// <param name="indexBegin">Starting bit location</param>
        /// <param name="data">The full raw data</param>
        /// <param name="toWrite">The integer to write on 32 bits</param>
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

        /// <summary>
        /// Write n byte of toWrite in data, starting at indexBegin
        /// </summary>
        /// <param name="indexBegin">Starting bit location</param>
        /// <param name="data">The full raw data</param>
        /// <param name="toWrite">The integer to write</param>
        /// <param name="n">Number of byte to write</param>
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

        /// <summary>
        /// Read a single bit in data at location index
        /// </summary>
        /// <param name="index">Location of the bit</param>
        /// <param name="data">The full raw data</param>
        /// <returns>Bit read</returns>
        public static bool ReadBit(int index, byte[] data)
        {
            if (data.Length <= index / 8)
                throw new AccessViolationException($"QUIC packet too small (size: {data.Length * 8}, reading at: {index})");

            // True if the bit n index is true
            return (data[index / 8] >> (7 - (index % 8))) % 2 == 1;
        }

        /// <summary>
        /// Read n bits in data, starting at indexBegin
        /// </summary>
        /// <param name="indexBegin">Starting bit location</param>
        /// <param name="data">The full raw data</param>
        /// <param name="n">Number of bits to read</param>
        /// <returns>Integer read</returns>
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

        /// <summary>
        /// Read n bits in data, starting at indexBegin
        /// </summary>
        /// <param name="indexBegin">Starting bit location</param>
        /// <param name="data">The full raw data</param>
        /// <param name="n">Number of bits to read</param>
        /// <returns>Integer read</returns>
        public static ulong LongReadNBits(int indexBegin, byte[] data, uint n)
        {
            return LongReadNBits(indexBegin, data, (int)n);
        }

        /// <summary>
        /// Read n bits in data, starting at indexBegin
        /// </summary>
        /// <param name="indexBegin">Starting bit location</param>
        /// <param name="data">The full raw data</param>
        /// <param name="n">Number of bits to read</param>
        /// <returns>Integer read</returns>
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

        /// <summary>
        /// Read n bits in data, starting at indexBegin
        /// </summary>
        /// <param name="indexBegin">Starting bit location</param>
        /// <param name="data">The full raw data</param>
        /// <param name="n">Number of bits to read</param>
        /// <returns>Integer read</returns>
        public static uint ReadNBits(int indexBegin, byte[] data, uint n)
        {
            return ReadNBits(indexBegin, data, (int)n);
        }

        /// <summary>
        /// Read a byte in data starting at indexBegin
        /// </summary>
        /// <param name="indexBegin">Starting bit location</param>
        /// <param name="data">The full raw data</param>
        /// <returns>Integer read</returns>
        public static uint ReadByte(int indexBegin, byte[] data)
        {
            return ReadNBits(indexBegin, data, 8);
        }

        /// <summary>
        /// Read a n bytes in data starting at indexBegin
        /// </summary>
        /// <param name="indexBegin">Starting bit location</param>
        /// <param name="data">The full raw data</param>
        /// <param name="n">Number of byte to read</param>
        /// <returns>Integer read</returns>
        // TODO: n should be a ulong, not an int
        public static ulong ReadNBytes(int indexBegin, byte[] data, int n)
        {
            return LongReadNBits(indexBegin, data, n * 8);
        }

        /// <summary>
        /// Read a n bytes in data starting at indexBegin
        /// </summary>
        /// <param name="indexBegin">Starting bit location</param>
        /// <param name="data">The full raw data</param>
        /// <param name="n">Number of byte to read</param>
        /// <returns>Integer read</returns>
        public static ulong ReadNBytes(int indexBegin, byte[] data, uint n)
        {
            return ReadNBytes(indexBegin, data, (int)n);
        }

        /// <summary>
        /// Read an unsigned int integer on 32 bits in data starting at indexBegin
        /// </summary>
        /// <param name="indexBegin">Starting bit location</param>
        /// <param name="data">The full raw data</param>
        /// <returns>Integer read</returns>
        public static uint ReadUInt32(int indexBegin, byte[] data)
        {
            uint ret = ReadNBits(indexBegin, data, 32);

            return ret;
        }

        /// <summary>
        /// Read an unsigned int integer on 64 bits in data starting at indexBegin
        /// </summary>
        /// <param name="indexBegin">Starting bit location</param>
        /// <param name="data">The full raw data</param>
        /// <returns>Integer read</returns>
        public static ulong ReadUInt64(int indexBegin, byte[] data)
        {
            ulong ret = LongReadNBits(indexBegin, data, 64);

            return ret;
        }
    }
}
