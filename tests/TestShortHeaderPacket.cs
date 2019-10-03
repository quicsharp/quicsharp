using Microsoft.VisualStudio.TestTools.UnitTesting;

using quicsharp;
using quicsharp.Frames;
using System;

namespace quicsharp.tests
{
    [TestClass]
    public class TestShortHeaderPacket
    {
        [TestMethod]
        public void TestEncode()
        {
            ShortHeaderPacket shp = new ShortHeaderPacket();

            shp.AddFrame(new DebugFrame{Message = "Message" });

            byte[] pack = shp.Encode();

            Assert.AreEqual(pack[0] & 0x80, 0); // First bit
            Assert.AreEqual(pack[0] & 0x40, 0x40); // Second bit
        }
    }
}
