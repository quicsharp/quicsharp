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
        public void TestInitialPacket()
        {
            InitialPacket sentP = new InitialPacket { DCIDLength = 4, DCID = 4321, PacketNumberLength = 2, SCIDLength = 4, SCID = 1045, PacketNumber = 1234, TokenLength = new VariableLengthInteger(3), Token = 4242};

            byte[] b = sentP.Encode();

            Packet p = Packet.Unpack(b);

            Assert.AreEqual(p.GetType(), typeof(InitialPacket));
            InitialPacket recP = p as InitialPacket;

            Assert.AreEqual((UInt32)4, recP.DCIDLength);
            Assert.AreEqual((UInt32)4321, recP.DCID);
            Assert.AreEqual((UInt32)4, recP.SCIDLength);
            Assert.AreEqual((UInt32)1045, recP.SCID);
            Assert.AreEqual((UInt32)2, recP.PacketNumberLength);
            Assert.AreEqual((UInt32)1234, recP.PacketNumber);
            Assert.AreEqual((UInt64)3, recP.TokenLength.Value);
            Assert.AreEqual((UInt32)4242, recP.Token);
            Assert.AreEqual((UInt64)24, recP.Length.Value);
        }

        [TestMethod]
        public void TestHandshakePacket()
        {
            HandshakePacket sentP = new HandshakePacket
            {
                DCIDLength = 4,
                DCID = 6789,
                PacketNumberLength = 3,
                SCIDLength = 4,
                SCID = 2356,
                PacketNumber = 91235,
            };

            byte[] b = sentP.Encode();

            Packet p = Packet.Unpack(b);

            Assert.AreEqual(p.GetType(), typeof(HandshakePacket));
            HandshakePacket recP = p as HandshakePacket;

            Assert.AreEqual((UInt32)4, recP.DCIDLength);
            Assert.AreEqual((UInt32)6789, recP.DCID);
            Assert.AreEqual((UInt32)4, recP.SCIDLength);
            Assert.AreEqual((UInt32)2356, recP.SCID);
            Assert.AreEqual((UInt32)3, recP.PacketNumberLength);
            Assert.AreEqual((UInt32)91235, recP.PacketNumber);
            Assert.AreEqual((UInt64)20, recP.Length.Value);
        }

        [TestMethod]
        public void TestRetryPacket()
        {
            RetryPacket sentP = new RetryPacket
            {
                DCIDLength = 4,
                DCID = 6789,
                SCIDLength = 4,
                SCID = 3104,
                ODCIDLength = 4,
                ODCID = 12345,
                RetryToken = new byte[] { 0x12, 0x45, 0x76, 0xf2 },
            };

            byte[] b = sentP.Encode();

            Packet p = Packet.Unpack(b);

            Assert.AreEqual(p.GetType(), typeof(RetryPacket));
            RetryPacket recP = p as RetryPacket;

            Assert.AreEqual((UInt32)4, recP.DCIDLength);
            Assert.AreEqual((UInt32)6789, recP.DCID);
            Assert.AreEqual((UInt32)4, recP.SCIDLength);
            Assert.AreEqual((UInt32)3104, recP.SCID);
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
                DCID = 1240,
                SCIDLength = 4,
                SCID = 55621,
            };

            byte[] b = sentP.Encode();

            Packet p = Packet.Unpack(b);

            Assert.AreEqual(p.GetType(), typeof(RTTPacket));
            RTTPacket recP = p as RTTPacket;

            Assert.AreEqual((UInt32)4, recP.DCIDLength);
            Assert.AreEqual((UInt32)1240, recP.DCID);
            Assert.AreEqual((UInt32)4, recP.SCIDLength);
            Assert.AreEqual((UInt32)55621, recP.SCID);
        }
    }
}
