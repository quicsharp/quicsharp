using Microsoft.VisualStudio.TestTools.UnitTesting;

using quicsharp;
using quicsharp.Frames;
using System;

namespace quicsharp.tests
{
    [TestClass]
    public class TestLongHeaderPacket
    {


        [TestMethod]
        public void TestRetryPacket()
        {
            RetryPacket sentP = new RetryPacket
            {
                DCIDLength = 4,
                DCID = new byte[] { 0x00, 0x00, 0x1a, 0x85 },
                SCIDLength = 4,
                SCID = new byte[] { 0x00, 0x00, 0x0c, 0x20 },
                ODCIDLength = 4,
                ODCID = 12345,
                RetryToken = new byte[] { 0x12, 0x45, 0x76, 0xf2 },
            };

            byte[] b = sentP.Encode();

            Packet p = Packet.Unpack(b);

            Assert.AreEqual(p.GetType(), typeof(RetryPacket));
            RetryPacket recP = p as RetryPacket;

            Assert.AreEqual((UInt32)4, recP.DCIDLength);
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x1a, 0x85 }, recP.DCID);
            Assert.AreEqual((UInt32)4, recP.SCIDLength);
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x0c, 0x20 }, recP.SCID);
            Assert.AreEqual((UInt32)4, recP.ODCIDLength);
            Assert.AreEqual((UInt32)12345, recP.ODCID);
            CollectionAssert.AreEqual(new byte[] { 0x12, 0x45, 0x76, 0xf2 }, recP.RetryToken);
        }

        [TestMethod]
        public void TestRTTPacket()
        {
            RTTPacket sentP = new RTTPacket
            {
                DCIDLength = 4,
                DCID = new byte[] { 0x00, 0x00, 0x04, 0xd8 },
                SCIDLength = 4,
                SCID = new byte[] { 0x00, 0x00, 0xd9, 0x45 },
            };

            byte[] b = sentP.Encode();

            Packet p = Packet.Unpack(b);

            Assert.AreEqual(p.GetType(), typeof(RTTPacket));
            RTTPacket recP = p as RTTPacket;

            Assert.AreEqual((UInt32)4, recP.DCIDLength);
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x04, 0xd8 }, recP.DCID);
            Assert.AreEqual((UInt32)4, recP.SCIDLength);
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0xd9, 0x45 }, recP.SCID);
        }
    }
}
