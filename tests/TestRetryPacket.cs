using Microsoft.VisualStudio.TestTools.UnitTesting;

using quicsharp;
using quicsharp.Frames;
using System;

namespace quicsharp.tests
{
    [TestClass]
    public class TestRetryPacket
    {
        [TestMethod]
        public void TestEncodeAndDecode()
        {
            RetryPacket sentP = new RetryPacket
            {
                DCIDLength_ = 2,
                DCID_ = new byte[] { 0x1a, 0x85 },
                SCIDLength_ = 2,
                SCID_ = new byte[] { 0x0c, 0x20 },
                ODCIDLength = 3,
                ODCID = new byte[] { 0x12, 0x34, 0x56 },
                RetryToken = new byte[] { 0x12, 0x45, 0x76, 0xf2 },
            };

            byte[] b = sentP.Encode();

            Packet p = Packet.Unpack(b);

            Assert.AreEqual(p.GetType(), typeof(RetryPacket));
            RetryPacket recP = p as RetryPacket;

            Assert.AreEqual((UInt32)2, recP.DCIDLength_);
            CollectionAssert.AreEqual(new byte[] { 0x1a, 0x85 }, recP.DCID_);
            Assert.AreEqual((UInt32)2, recP.SCIDLength_);
            CollectionAssert.AreEqual(new byte[] { 0x0c, 0x20 }, recP.SCID_);
            Assert.AreEqual((UInt32)3, recP.ODCIDLength);
            CollectionAssert.AreEqual(new byte[] { 0x12, 0x34, 0x56 }, recP.ODCID);
            CollectionAssert.AreEqual(new byte[] { 0x12, 0x45, 0x76, 0xf2 }, recP.RetryToken);
        }
    }
}
