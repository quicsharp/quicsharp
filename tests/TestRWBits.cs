using Microsoft.VisualStudio.TestTools.UnitTesting;

using quicsharp;
using System;

namespace quicsharp.tests
{
    [TestClass]
    public class TestRWBits
    {
        [TestMethod]
        public void TestWriteBit()
        {
            byte[] b = new byte[] { 0, 0, 0 };

            BitUtils.WriteBit(10, b, true);

            Assert.AreEqual(b[1], (1 << 5));
        }

        [TestMethod]
        public void TestReadBit()
        {
            byte[] b = new byte[] { 0, 64, 0 };

            for (int i = 0; i < 24; i++)
            {
                if (i == 9)
                    Assert.IsTrue(BitUtils.ReadBit(i, b));
                else
                    Assert.IsFalse(BitUtils.ReadBit(i, b));
            }
        }

        [TestMethod]
        public void TestWriteNBits()
        {
            byte[] b = new byte[] { 0, 0, 0 };

            bool[] t = new bool[] { true, true, false, false, true, false, false, true };

            BitUtils.WriteNBits(8, b, t);

            Assert.AreEqual(b[1], (uint)((1 << 7) + (1 << 6) + (1 << 3) + 1));
        }

        [TestMethod]
        public void TestWriteNByteFromInt()
        {
            byte[] b = new byte[] { 0, 0, 0 };

            BitUtils.WriteNByteFromInt(0, b, (uint)201, 2);
            Assert.AreEqual(b[1], (uint)201);
            BitUtils.WriteNByteFromInt(0, b, (uint)521, 2);
            Assert.AreEqual(b[0], (uint)2);
            Assert.AreEqual(b[1], (uint)9);
            Assert.ThrowsException<ArgumentException>(() => BitUtils.WriteNByteFromInt(0, b, (uint)Math.Pow(2, 16), 2));
            Assert.ThrowsException<AccessViolationException>(() => BitUtils.WriteNByteFromInt(16, b, (uint)Math.Pow(2, 14), 2));
        }

        [TestMethod]
        public void TestLongReadNBits()
        {
            byte[] b = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };

            Assert.AreEqual((ulong)Math.Pow(2, 56), BitUtils.LongReadNBits(0, b, 64));
            Assert.AreEqual(BitUtils.LongReadNBits(0, b, 64), BitUtils.LongReadNBits(0, b, (uint)64));

        }

        [TestMethod]
        public void TestReadNBits()
        {
            byte[] b = new byte[] { 1, 0, 0, 0 };

            Assert.AreEqual((uint)Math.Pow(2, 24), BitUtils.ReadNBits(0, b, 32));
            Assert.AreEqual((uint)Math.Pow(2, 16), BitUtils.ReadNBits(0, b, 24));
            Assert.AreEqual(BitUtils.ReadNBits(0, b, 32), BitUtils.LongReadNBits(0, b, (uint)32));
            Assert.AreEqual((uint)Math.Pow(2, 3), BitUtils.ReadNBits(7, b, 4));
            Assert.ThrowsException<AccessViolationException>(() => BitUtils.ReadNBits(0, b, 35));
        }

        [TestMethod]
        public void TestReadByte()
        {
            byte[] b = new byte[] { 1, 0, 1, 96 };

            Assert.AreEqual(b[0], BitUtils.ReadByte(0, b));
            // ReadByte allows us to read a full Byte (8 Bits) from anywhere in a Byte Array, provided there are enough bits remaining
            Assert.AreEqual((uint)(256 + 96) >> 1, BitUtils.ReadByte(23, b));
            Assert.ThrowsException<AccessViolationException>(() => BitUtils.ReadByte(28, b));
        }

        [TestMethod]
        public void TestReadNBytes()
        {
            byte[] b = new byte[] { 1, 0, 1, 96 };

            Assert.AreEqual(((ulong)b[0] << 8) + b[1], BitUtils.ReadNBytes(0, b, 2));
            Assert.AreEqual(BitUtils.ReadNBytes(0, b, 2), BitUtils.ReadNBytes(0, b, (uint)2));
            Assert.ThrowsException<AccessViolationException>(() => BitUtils.ReadNBytes(28, b, 2));
        }

        [TestMethod]
        public void ReadUInt64()
        {
            byte[] d = { 1, 0, 30, 0, 23, 200, 48, 0 };

            ulong v = ((ulong)d[0] << 56) + ((ulong)d[1] << 48) + ((ulong)d[2] << 40) + ((ulong)d[3] << 32) + ((ulong)d[4] << 24) + ((ulong)d[5] << 16) + ((ulong)d[6] << 8) + (ulong)d[7];

            Assert.AreEqual(v, BitUtils.ReadUInt64(0, d));
            Assert.ThrowsException<AccessViolationException>(() => BitUtils.ReadUInt64(8, d));
        }

        [TestMethod]
        public void TestWriteUInt32()
        {
            UInt32 n = 128;
            byte[] data = new byte[5];

            BitUtils.WriteUInt32(8, data, n);

            Assert.AreEqual(data[4], (1 << 7));
        }

        [TestMethod]
        public void TestReadUInt32()
        {
            byte[] b = new byte[] { 0, 64, 0, 0, 1, 0, 64};

            UInt32 n = BitUtils.ReadUInt32(16, b);

            Assert.AreEqual(n, (UInt32)256);
        }
    }
}
