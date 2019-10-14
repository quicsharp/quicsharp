using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

using quicsharp.Frames;
using System;

namespace quicsharp.tests
{
    [TestClass]
    public class TestAckFrame
    {
        [TestMethod]
        public void TestConstructor()
        {
            List<UInt32> l = new List<UInt32>() { 1, 2, 4, 6, 5, 10, 11, 12, 13 };
            AckFrame af = new AckFrame(l, 500);

            Assert.AreEqual((UInt64)2, af.AckRangeCount.Value);
            Assert.AreEqual((UInt64)13, af.LargestAcknowledged.Value);
            Assert.AreEqual((UInt64)4, af.FirstAckRange.Value);
            Assert.AreEqual((UInt64)500, af.Delay.Value);

            Assert.AreEqual((UInt64)0, af.ECT0.Value);
            Assert.AreEqual((UInt64)0, af.ECT1.Value);
            Assert.AreEqual((UInt64)0, af.ECN_CE.Value);
        }
        [TestMethod]
        public void TestEncodeDecode()
        {
            AckFrame af = new AckFrame
            {
                LargestAcknowledged = new VariableLengthInteger(32767),
                Delay = new VariableLengthInteger(500),
                AckRangeCount = new VariableLengthInteger(1),
                FirstAckRange = new VariableLengthInteger(10),
                AckRanges = new List<(VariableLengthInteger, VariableLengthInteger)> { (new VariableLengthInteger(3), new VariableLengthInteger(40)) },
                ECT0 = new VariableLengthInteger(1345),
                ECT1 = new VariableLengthInteger(1234),
                ECN_CE = new VariableLengthInteger(84),
            };

            byte[] b = af.Encode();

            AckFrame afDecoded = new AckFrame();
            Assert.AreEqual(b.Length * 8, afDecoded.Decode(b, 0));
            
            Assert.AreEqual((UInt64)32767, afDecoded.LargestAcknowledged.Value);
            Assert.AreEqual((UInt64)500, afDecoded.Delay.Value);
            Assert.AreEqual((UInt64)1, afDecoded.AckRangeCount.Value);
            Assert.AreEqual((UInt64)10, afDecoded.FirstAckRange.Value);
            Assert.AreEqual((UInt64)1345, afDecoded.ECT0.Value);
            Assert.AreEqual((UInt64)1234, afDecoded.ECT1.Value);
            Assert.AreEqual((UInt64)84, afDecoded.ECN_CE.Value);

            Assert.AreEqual(1, afDecoded.AckRanges.Count);
            Assert.AreEqual((UInt64)3, afDecoded.AckRanges[0].Item1.Value);
            Assert.AreEqual((UInt64)40, afDecoded.AckRanges[0].Item2.Value);
        }
    }
}