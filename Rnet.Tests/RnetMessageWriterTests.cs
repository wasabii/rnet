using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rnet.Tests
{

    [TestClass]
    public class RnetMessageWriterTests
    {

        [TestMethod]
        public void TestDeviceId()
        {
            var stm = new MemoryStream();
            var wrt = new RnetStreamWriter(stm);

            new RnetDeviceId(0x00, 0x00, 0x7f).Write(wrt);

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
            var wrt = new RnetStreamWriter(stm);

            new RnetPath().Write(wrt);

            Assert.IsTrue(stm.ToArray().SequenceEqual(new byte[] { 0x00 }));
        }

        [TestMethod]
        public void TestPath()
        {
            var stm = new MemoryStream();
            var wrt = new RnetStreamWriter(stm);

            new RnetPath(1, 2, 3).Write(wrt);

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
            var wrt = new RnetStreamWriter(stm);

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
            var wrt = new RnetStreamWriter(stm);

            wrt.BeginMessage(RnetDeviceId.RootController, RnetDeviceId.External, RnetMessageType.Event);
            RnetDeviceId.RootController.Write(wrt);
            new RnetPath(2,0).Write(wrt);
            new RnetPath().Write(wrt);
            wrt.WriteUInt16((ushort)RnetEvents.VolumeUp); // VOLUME UP
            wrt.WriteUInt16(0);
            wrt.WriteUInt16(0);
            wrt.WriteByte(1);
            wrt.EndMessage();

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
