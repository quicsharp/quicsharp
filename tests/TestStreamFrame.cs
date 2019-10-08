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
            Assert.AreEqual(sf._OFF, true);
            Assert.AreEqual(sf._LEN, false);
            Assert.AreEqual(sf._FIN, true);
            Assert.AreEqual(sf._streamID.Value, Convert.ToUInt64(37));
            Assert.AreEqual(sf._offset.Value, Convert.ToUInt64(494878333));
            CollectionAssert.AreEqual(sf._data, new byte[] { 0x42 });
        }

        [TestMethod]
        public void TestEncode()
        {
            byte[] expected = new byte[] { 0b00001110, 0x7b, 0xbd, 0x9d, 0x7f, 0x3e, 0x7d, 0x02, 0x42, 0x43 };

            UInt64 streamID = 15293;                    // 0x7b, 0xbd
            UInt64 offset = 494878333;                  // 0x9d, 0x7f, 0x3e, 0x7d
            byte[] data = new byte[] { 0x42, 0x43 };    // implicit length: 2
            bool isLastFrameOfPacket = false;           // length should be provided
            bool isEndOfStream = false;                 // FIN bit should be set to 0

            Frames.StreamFrame sf = new Frames.StreamFrame(streamID, offset, data, isLastFrameOfPacket, isEndOfStream);
            byte[] result = sf.Encode();
            CollectionAssert.AreEqual(result, expected);
        }
    }
}
