using Microsoft.VisualStudio.TestTools.UnitTesting;

using quicsharp.Frames;
using System;

namespace quicsharp.tests
{
    [TestClass]
    public class TestRTTPacket
    {

        [TestMethod]
        public void TestEncodeAndDecode()
        {
            RTTPacket sentP = new RTTPacket
            {
                DCIDLength = 2,
                DCID = new byte[] { 0x1a, 0x85 },
                PacketNumberLength = 3,
                SCIDLength = 2,
                SCID = new byte[] { 0x09, 0x34 },
                PacketNumber = 91235,
                Payload = new byte[] { 0x01, 0x10 },
            };

            byte[] b = sentP.Encode();

            Packet p = Packet.Unpack(b);

            Assert.AreEqual(p.GetType(), typeof(RTTPacket));
            RTTPacket recP = p as RTTPacket;

            Assert.AreEqual((UInt32)2, recP.DCIDLength);
            CollectionAssert.AreEqual(new byte[] { 0x1a, 0x85 }, recP.DCID);
            Assert.AreEqual((UInt32)2, recP.SCIDLength);
            CollectionAssert.AreEqual(new byte[] { 0x09, 0x34 }, recP.SCID);
            Assert.AreEqual((UInt32)3, recP.PacketNumberLength);
            Assert.AreEqual((UInt32)91235, recP.PacketNumber);
            Assert.AreEqual((UInt64)5, recP.Length.Value); // 5 = PacketNumberLength + Payload.Length
        }

        [TestMethod]
        public void TestDecode()
        {
            HexData samplePacket = new HexData("d4ff0000171116b9c8c2eb9a0c815d415be8bda522edfa145e1178e63dd8e8da9f9c637dd95b03413c82cd8c408e6f9722022f02eeb44626382118e6c9e0cb7df0099dbe936a71f0dbe85e4dd445d11f7cd46fa0f21d3972574164ed409c03a56f8585891a18652bd7682914647f4ce42c6fef68161e4a3321a870e369edccdfcc130a88f6f4b5e33d820dcc996db5d7cc775b29bf2f34cdd23d906f2d386026c4ce002dfdd227bc0fcee14a8089278e4e59c36ecc549c4f399b8c08");
            byte[] bytes = samplePacket.bytes;
            Packet p = Packet.Unpack(samplePacket.bytes);

            Assert.AreEqual(p.GetType(), typeof(RTTPacket));
            RTTPacket recP = p as RTTPacket;

            Assert.AreEqual(0xff000017, recP.Version);
            Assert.AreEqual((UInt32)17, recP.DCIDLength);
            CollectionAssert.AreEqual((new HexData("16b9c8c2eb9a0c815d415be8bda522edfa")).bytes, recP.DCID);
            Assert.AreEqual((UInt32)20, recP.SCIDLength);
            CollectionAssert.AreEqual((new HexData("5e1178e63dd8e8da9f9c637dd95b03413c82cd8c")).bytes, recP.SCID);

            // Something shady is going on with packet number length. See TestInitialPacket
            // TODO: fix this
            // Assert.AreEqual((UInt32)1, recP.PacketNumberLength); 
            // Assert.AreEqual((UInt32)111, recP.PacketNumber);
            Assert.AreEqual((UInt64)142, recP.Length.Value);
            CollectionAssert.AreEqual((new HexData("9722022f02eeb44626382118e6c9e0cb7df0099dbe936a71f0dbe85e4dd445d11f7cd46fa0f21d3972574164ed409c03a56f8585891a18652bd7682914647f4ce42c6fef68161e4a3321a870e369edccdfcc130a88f6f4b5e33d820dcc996db5d7cc775b29bf2f34cdd23d906f2d386026c4ce002dfdd227bc0fcee14a8089278e4e59c36ecc549c4f399b8c08")).bytes, recP.Payload);
        }
    }
}