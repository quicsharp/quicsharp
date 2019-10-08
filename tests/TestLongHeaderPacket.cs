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
            InitialPacket sentP = new InitialPacket { DCIDLength = 4, DCID = 4321, PacketNumberLength = 2, SCIDLength = 4, PacketNumber = 1234, TokenLength = new VariableLengthInteger(3), Token = 4242};

            byte[] b = sentP.Encode();

            Packet p = Packet.Unpack(b);

            Assert.AreEqual(p.GetType(), typeof(InitialPacket));
            InitialPacket recP = p as InitialPacket;

            Assert.AreEqual(4, recP.DCIDLength);
            Assert.AreEqual((UInt32)4321, recP.DCID);
            Assert.AreEqual(4, recP.SCIDLength);
            Assert.AreEqual(2, recP.PacketNumberLength);
            Assert.AreEqual((UInt32)1234, recP.PacketNumber);
            Assert.AreEqual((UInt64)3, recP.TokenLength.Value);
            Assert.AreEqual((UInt32)4242, recP.Token);
            Assert.AreEqual((UInt64)23, recP.Length.Value);
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
                PacketNumber = 91235,
            };

            byte[] b = sentP.Encode();

            Packet p = Packet.Unpack(b);

            Assert.AreEqual(p.GetType(), typeof(HandshakePacket));
            HandshakePacket recP = p as HandshakePacket;

            Assert.AreEqual(4, recP.DCIDLength);
            Assert.AreEqual((UInt32)6789, recP.DCID);
            Assert.AreEqual(4, recP.SCIDLength);
            Assert.AreEqual(3, recP.PacketNumberLength);
            Assert.AreEqual((UInt32)91235, recP.PacketNumber);
            Assert.AreEqual((UInt64)19, recP.Length.Value);
        }
    }
}
