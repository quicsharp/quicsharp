﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            InitialPacket sentP = new InitialPacket { ClientId = 1, DCIDLength = 2, DCID = 4321, PacketNumberLength = 2, PacketNumber = 1234, TokenLength = new VariableLengthInteger(3), Token = 4242, Length = new VariableLengthInteger(5)};

            byte[] b = sentP.Encode();

            Packet p = Packet.Unpack(b);

            Assert.AreEqual(p.GetType(), typeof(InitialPacket));
            InitialPacket recP = p as InitialPacket;

            Assert.AreEqual(1, recP.ClientId);
            Assert.AreEqual(2, recP.DCIDLength);
            Assert.AreEqual(4321, recP.DCID);
            Assert.AreEqual(2, recP.PacketNumberLength);
            Assert.AreEqual(1234, recP.PacketNumber);
            Assert.AreEqual(3, recP.TokenLength.Value);
            Assert.AreEqual(4242, recP.Token);
            Assert.AreEqual(5, recP.Length.Value);
        }
    }
}
