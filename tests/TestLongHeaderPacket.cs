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
            InitialPacket sentP = new InitialPacket { DCIDLength = 4, DCID = 4321, PacketNumberLength = 2, SCIDLength = 4, PacketNumber = 1234, TokenLength = new VariableLengthInteger(3), Token = 4242, Length = new VariableLengthInteger(5)};

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
            Assert.AreEqual((UInt64)5, recP.Length.Value);
        }
    }
}
