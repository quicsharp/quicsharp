using Microsoft.VisualStudio.TestTools.UnitTesting;

using quicsharp;
using quicsharp.Frames;
using System;
using System.Collections.Generic;

namespace quicsharp.tests
{
    [TestClass]
    public class TestShortHeaderPacket
    {
        [TestMethod]
        public void TestEncode()
        {
            ShortHeaderPacket shp = new ShortHeaderPacket();

            shp.PacketNumber = 42;
            shp.DestinationConnectionID = 123;
            shp.AddFrame(new DebugFrame { Message = "Message" });

            byte[] pack = shp.Encode();

            /* 
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
               | 0 | 1 | S | R | R | K | P P |
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
               | Destination Connection ID(0..160)           ...
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
               | Packet Number(8 / 16 / 24 / 32)...
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
               | Protected Payload(*)...
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
            */
            Assert.AreEqual(pack.Length, 17);

            Assert.AreEqual(pack[0] & 0x80, 0); // First bit
            Assert.AreEqual(pack[0] & 0x40, 0x40); // Second bit
            Assert.AreEqual(pack[0] & 0x20, 0);
            Assert.AreEqual(pack[0] & 0x04, 0);
            Assert.AreEqual(pack[0] & 0x02, 0x02);
            Assert.AreEqual(pack[0] & 0x01, 0x01);

            Assert.AreEqual(pack[1], 0);
            Assert.AreEqual(pack[2], 0);
            Assert.AreEqual(pack[3], 0);
            Assert.AreEqual(pack[4], 123);

            Assert.AreEqual(pack[5], 0);
            Assert.AreEqual(pack[6], 0);
            Assert.AreEqual(pack[7], 0);
            Assert.AreEqual(pack[8], 42);

            // Payload
            Assert.AreEqual(pack[9], 0x1e); // Type Debug
            Assert.AreEqual(pack[10], Convert.ToByte('M'));
            Assert.AreEqual(pack[11], Convert.ToByte('e'));
            Assert.AreEqual(pack[12], Convert.ToByte('s'));
            Assert.AreEqual(pack[13], Convert.ToByte('s'));
            Assert.AreEqual(pack[14], Convert.ToByte('a'));
            Assert.AreEqual(pack[15], Convert.ToByte('g'));
            Assert.AreEqual(pack[16], Convert.ToByte('e'));
        }

        [TestMethod]
        public void TestDecode()
        {
            ShortHeaderPacket shp = new ShortHeaderPacket();

            shp.PacketNumber = 42;
            shp.DestinationConnectionID = 123;
            shp.AddFrame(new DebugFrame { Message = "Message" });

            byte[] pack = shp.Encode();

            Packet p = Packet.Unpack(pack);

            Assert.AreEqual(p.GetType(), typeof(ShortHeaderPacket));
            ShortHeaderPacket sh = p as ShortHeaderPacket;

            Assert.AreEqual(p.ClientId, (UInt32)42);
            Assert.AreEqual(sh.DestinationConnectionID, (UInt64)123);
            Assert.AreEqual(sh.PacketNumber, (UInt64)42);
            Assert.AreEqual(sh.Spin, false);
            Assert.AreEqual(sh.KeyPhase, false);
            Assert.AreEqual(sh.PacketNumberLengthByte, 4);

            foreach (Frame f in p.Frames)
            {
                Assert.AreEqual(f.Type, 0x1e);
                DebugFrame fd = f as DebugFrame;
                Assert.AreEqual(fd.Message, "Message");
            }
        }
    }
}
