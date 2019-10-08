using Microsoft.VisualStudio.TestTools.UnitTesting;

using quicsharp;
using System;

namespace quicsharp.tests
{
    [TestClass]
    public class TestVariableLengthInteger
    {
        [TestMethod]
        public void TestEncode()
        {
            VariableLengthInteger n = new VariableLengthInteger((UInt64)61);
            Assert.AreEqual((UInt64)61, n.Value);
            Assert.AreEqual(8, n.Size);
            byte[] b = n.Encode();
            Assert.AreEqual(1, b.Length);
            Assert.AreEqual(61, b[0]);

            n.Value = 1414;
            Assert.AreEqual((UInt64)1414, n.Value);
            Assert.AreEqual(16, n.Size);
            b = n.Encode();
            Assert.AreEqual(2, b.Length);
            Assert.AreEqual(0x40, b[0] & 0xc0);
            Assert.AreEqual(0x05, b[0] % 0x40); // 1024 + 256
            Assert.AreEqual(134, b[1]);

            n.Value = 65536;
            Assert.AreEqual((UInt64)65536, n.Value);
            Assert.AreEqual(32, n.Size);
            b = n.Encode();
            Assert.AreEqual(4, b.Length);
            Assert.AreEqual(0x80, b[0] & 0xc0);
            Assert.AreEqual(0, b[0] % 0x40);
            Assert.AreEqual(1, b[1]);
            Assert.AreEqual(0, b[2]);
            Assert.AreEqual(0, b[3]);

            n.Value = (1UL << 32);
            Assert.AreEqual(1UL << 32, n.Value);
            Assert.AreEqual(64, n.Size);
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
            byte[] b = new byte[] { 0, 0, 3, 0 };

            Assert.AreEqual(8, n.Decode(16, b));
            Assert.AreEqual(8, n.Size);
            Assert.AreEqual((UInt64)3, n.Value);

            b = new byte[] { 0, 128, 1, 0, 0, 0, 0 };
            Assert.AreEqual(32, n.Decode(8, b));
            Assert.AreEqual(32, n.Size);
            Assert.AreEqual((UInt64)65536, n.Value);

            b = new byte[] { 0xc2, 0x19, 0x7c, 0x5e, 0xff, 0x14, 0xe8, 0x8c };
            Assert.AreEqual(64, n.Decode(0, b));
            Assert.AreEqual(8, n.Size);
            Assert.AreEqual((UInt64)151288809941952652, n.Value);

            b = new byte[] { 0x9d, 0x7f, 0x3e, 0x7d };
            Assert.AreEqual(32, n.Decode(0, b));
            Assert.AreEqual(4, n.Size);
            Assert.AreEqual((UInt64)494878333, n.Value);

            b = new byte[] { 0x7b, 0xbd };
            Assert.AreEqual(16, n.Decode(0, b));
            Assert.AreEqual(2, n.Size);
            Assert.AreEqual((UInt64)15293, n.Value);

            b = new byte[] { 0x25 };
            Assert.AreEqual(8, n.Decode(0, b));
            Assert.AreEqual(1, n.Size);
            Assert.AreEqual((UInt64)37, n.Value);

            b = new byte[] { 0x40, 0x25 };
            Assert.AreEqual(16, n.Decode(0, b));
            Assert.AreEqual(2, n.Size);
            Assert.AreEqual((UInt64)37, n.Value);
        }
    }
}
