using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

using quicsharp;
using quicsharp.Frames;
using System;

namespace quicsharp.tests
{
    [TestClass]
    public class TestPacketManager
    {
        [TestMethod]
        public void TestProcessAckFrame()
        {
            AckFrame af = new AckFrame
            {
                LargestAcknowledged = new VariableLengthInteger(50),
                Delay = new VariableLengthInteger(500),
                AckRangeCount = new VariableLengthInteger(1),
                FirstAckRange = new VariableLengthInteger(7),
                AckRanges = new List<(VariableLengthInteger, VariableLengthInteger)> { (new VariableLengthInteger(3), new VariableLengthInteger(20)) },
                ECT0 = new VariableLengthInteger(1345),
                ECT1 = new VariableLengthInteger(1234),
                ECN_CE = new VariableLengthInteger(84),
            };

            PacketManager pm = new PacketManager(0, 0);

            for (UInt32 i = 1; i < 70; i++)
            {
                ShortHeaderPacket pack = new ShortHeaderPacket();
                pack.PacketNumber = i;
                pm.Register(pack, i);
            }

            pm.ProcessAckFrame(af);
            Assert.AreEqual(42, pm.History.Count);
            foreach (KeyValuePair<UInt32, Packet> t in pm.History)
            {
                Assert.IsTrue(t.Key <= 20 || (t.Key > 40 && t.Key < 44) || t.Key > 50);
            }
        }
    }
}
