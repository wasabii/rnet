using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rnet.Tests
{

    [TestClass]
    public class RnetMessageWriterTests
    {

        [TestMethod]
        public void TestBasicMessage()
        {
            var stm = new MemoryStream();
            var wrt = new RnetMessageWriter(stm);

            // message without start/end/checksum
            wrt.WriteMessage(
                0x00,
                0x00,
                0x7F,
                0x00,
                0x00,
                0x70,
                0x05,
                0x02,
                0x02,
                0x00,
                0x00,
                0x7F,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x01);

            // expected volumn up message
            var expected = new byte[] {
                0xF0,
                0x00,
                0x00,
                0x7F,
                0x00,
                0x00,
                0x70,
                0x05,
                0x02,
                0x02,
                0x00,
                0x00,
                0x7F,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x01,
                0x7B,
                0xF7,
            };

            Assert.IsTrue(stm.ToArray().SequenceEqual(expected));
        }

        [TestMethod]
        public void TestDeviceId()
        {
            var stm = new MemoryStream();
            var wrt = new RnetMessageWriter(stm);

            wrt.WriteDeviceId(new RnetDeviceId(0x00, 0x00, 0x7f));

            var expected = new byte[]
            {
                0x00,
                0x00,
                0x7f,
            };

            Assert.IsTrue(stm.ToArray().SequenceEqual(expected));
        }

        [TestMethod]
        public void TestEmptyPath()
        {
            var stm = new MemoryStream();
            var wrt = new RnetMessageWriter(stm);

            wrt.WritePath(null);

            Assert.IsTrue(stm.ToArray().SequenceEqual(new byte[] { 0x00 }));
        }

        [TestMethod]
        public void TestPath()
        {
            var stm = new MemoryStream();
            var wrt = new RnetMessageWriter(stm);

            wrt.WritePath(new RnetPath(1).Next(2).Next(3));

            var expected = new byte[]
            {
                0x03,
                0x01,
                0x02,
                0x03,
            };

            Assert.IsTrue(stm.ToArray().SequenceEqual(expected));
        }

        [TestMethod]
        public void TestUInt16()
        {
            var stm = new MemoryStream();
            var wrt = new RnetMessageWriter(stm);

            wrt.WriteUInt16(1);

            var expected = new byte[]
            {
                0x01,
                0x00,
            };

            Assert.IsTrue(stm.ToArray().SequenceEqual(expected)); 
        }

        [TestMethod]
        public void TestFullMessage()
        {
            var stm = new MemoryStream();
            var wrt = new RnetMessageWriter(stm);

            wrt.WriteStart();
            wrt.WriteDeviceId(RnetDeviceId.RootControllerTarget);
            wrt.WriteDeviceId(RnetDeviceId.ExternalSource);
            wrt.WriteMessageType(RnetMessageType.Event);
            wrt.WritePath(new RnetPath(2).Next(0));
            wrt.WritePath(null);
            wrt.WriteUInt16((ushort)RnetEvents.VolumeUp); // VOLUME UP
            wrt.WriteUInt16(0);
            wrt.WriteUInt16(0);
            wrt.WriteByte(1);
            wrt.WriteChecksum();
            wrt.WriteEnd();

            // expected volumn up message
            var expected = new byte[] {
                0xF0,
                0x00,
                0x00,
                0x7F,
                0x00,
                0x00,
                0x70,
                0x05,
                0x02,
                0x02,
                0x00,
                0x00,
                0x7F,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x01,
                0x7B,
                0xF7,
            };

            var actual = stm.ToArray();

            Assert.IsTrue(actual.SequenceEqual(expected)); 
        }

    }

}
