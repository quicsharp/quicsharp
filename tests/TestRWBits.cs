using Microsoft.VisualStudio.TestTools.UnitTesting;

using quicsharp;
using System;

namespace quicsharp.tests
{
    [TestClass]
    public class TestRWBits
    {
        [TestMethod]
        public void TestReadBit()
        {
            byte[] b = new byte[] { 0, 0, 0 };

            Packet.WriteBit(10, b, true);

            Assert.AreEqual(b[1], (1 << 5));
        }

        [TestMethod]
        public void TestWriteBit()
        {
            byte[] b = new byte[] { 0, 64, 0 };

            for (int i = 0; i < 24; i++)
            {
                if (i == 9)
                    Assert.IsTrue(Packet.ReadBit(i, b));
                else
                    Assert.IsFalse(Packet.ReadBit(i, b));
            }
        }

        [TestMethod]
        public void TestWriteUInt32()
        {
            UInt32 n = 128;
            byte[] data = new byte[5];

            Packet.WriteUInt32(8, data, n);

            Assert.AreEqual(data[4], (1 << 7));
        }

        [TestMethod]
        public void TestReadUInt32()
        {
            byte[] b = new byte[] { 0, 64, 0, 0, 1, 0, 64};

            UInt32 n = Packet.ReadUInt32(16, b);

            Assert.AreEqual(n, (UInt32)256);
        }
    }
}
