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
            Assert.AreEqual(8 * 8, sf.Decode(b, 3 * 8));
            Assert.AreEqual(sf.OFF, true);
            Assert.AreEqual(sf.LEN, false);
            Assert.AreEqual(sf.FIN, true);
            Assert.AreEqual(sf.StreamID.Value, Convert.ToUInt64(37));
            Assert.AreEqual(sf.Offset.Value, Convert.ToUInt64(494878333));
            CollectionAssert.AreEqual(sf.Data, new byte[] { 0x42 });
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

        [TestMethod]
        public void TestEncodeDecode()
        {
            UInt64 streamID = 15293;                    // 0x7b, 0xbd
            UInt64 offset = 494878333;                  // 0x9d, 0x7f, 0x3e, 0x7d
            byte[] data = new byte[] { 0x42, 0x43 };    // implicit length: 2
            bool isLastFrameOfPacket = false;           // length should be provided
            bool isEndOfStream = false;                 // FIN bit should be set to 0

            Frames.StreamFrame sentsf = new Frames.StreamFrame(streamID, offset, data, isLastFrameOfPacket, isEndOfStream);
            byte[] result = sentsf.Encode();

            Frames.StreamFrame recvsf = new Frames.StreamFrame();
            recvsf.Decode(result, 0);

            Assert.AreEqual(recvsf.OFF, true);
            Assert.AreEqual(recvsf.LEN, true);
            Assert.AreEqual(recvsf.FIN, isEndOfStream);
            Assert.AreEqual(recvsf.StreamID.Value, Convert.ToUInt64(15293));
            Assert.AreEqual(recvsf.Offset.Value, Convert.ToUInt64(494878333));
            CollectionAssert.AreEqual(recvsf.Data, new byte[] { 0x42, 0x43 });
        }
    }
}
