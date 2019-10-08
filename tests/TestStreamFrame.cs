using Microsoft.VisualStudio.TestTools.UnitTesting;

using quicsharp.Frames;
using System;

namespace quicsharp.tests
{
    [TestClass]
    public class TestStreamFrame
    {
        [TestMethod]
        public void TestDecode()
        {
            byte[] b = new byte[] { 0, 0, 0, 0b00001101, 0x40, 0x25, 0x9d, 0x7f, 0x3e, 0x7d, 0x42 };
            // Stream ID: 40 25 = 37
            // Offset: 9d 7f 3e 7d = 494878333

            Frames.StreamFrame sf = new Frames.StreamFrame();
            sf.Decode(b, 3);
            Assert.AreEqual(sf.OFF, true);
            Assert.AreEqual(sf.LEN, false);
            Assert.AreEqual(sf.FIN, true);
            Assert.AreEqual(sf.streamID.Value, Convert.ToUInt64(37));
            Assert.AreEqual(sf.offset.Value, Convert.ToUInt64(494878333));
            CollectionAssert.AreEqual(sf.data, new byte[] { 0x42 });
        }
    }
}
