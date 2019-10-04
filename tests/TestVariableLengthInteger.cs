using Microsoft.VisualStudio.TestTools.UnitTesting;

using quicsharp;
using System;

namespace Tests
{
    [TestClass]
    public class TestVariableLengthInteger
    {
        [TestMethod]
        public void TestEncode()
        {
            VariableLengthInteger n = new VariableLengthInteger((UInt64)61);
            Assert.AreEqual((UInt64)61, n.Value);
            Assert.AreEqual(1, n.Size);
            byte[] b = n.Encode();
            Assert.AreEqual(1, b.Length);
            Assert.AreEqual(61, b[0]);

            n.Value = 1414;
            Assert.AreEqual((UInt64)1414, n.Value);
            Assert.AreEqual(2, n.Size);
            b = n.Encode();
            Assert.AreEqual(2, b.Length);
            Assert.AreEqual(0x40, b[0] & 0xc0);
            Assert.AreEqual(0x05, b[0] % 0x40); // 1024 + 256
            Assert.AreEqual(134, b[1]);

            n.Value = 65536;
            Assert.AreEqual((UInt64)65536, n.Value);
            Assert.AreEqual(4, n.Size);
            b = n.Encode();
            Assert.AreEqual(4, b.Length);
            Assert.AreEqual(0x80, b[0] & 0xc0);
            Assert.AreEqual(0, b[0] % 0x40);
            Assert.AreEqual(1, b[1]);
            Assert.AreEqual(0, b[2]);
            Assert.AreEqual(0, b[3]);

            n.Value = (1UL << 32);
            Assert.AreEqual(1UL << 32, n.Value);
            Assert.AreEqual(8, n.Size);
            b = n.Encode();
            Assert.AreEqual(8, b.Length);
            Assert.AreEqual(0xc0, b[0] & 0xc0);
            Assert.AreEqual(0, b[0] % 0x40);
            Assert.AreEqual(0, b[1]);
            Assert.AreEqual(0, b[2]);
            Assert.AreEqual(1, b[3]);
            Assert.AreEqual(0, b[4]);
            Assert.AreEqual(0, b[5]);
            Assert.AreEqual(0, b[6]);
            Assert.AreEqual(0, b[7]);
        }

        [TestMethod]
        public void TestDecode()
        {
            VariableLengthInteger n = new VariableLengthInteger(0);
            byte[] b = new byte[] {0, 0, 3, 0 };

            Assert.AreEqual(8, n.Decode(16, b));
            Assert.AreEqual(1, n.Size);
            Assert.AreEqual((UInt64)3, n.Value);

            b = new byte[] { 0, 128, 1, 0, 0, 0, 0 };
            Assert.AreEqual(32, n.Decode(8, b));
            Assert.AreEqual(4, n.Size);
            Assert.AreEqual((UInt64)65536, n.Value);
        }
    }
}
