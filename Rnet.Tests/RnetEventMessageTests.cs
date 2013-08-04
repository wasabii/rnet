using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rnet.Protocol;

namespace Rnet.Protocol.Tests
{

    [TestClass]
    public class RnetEventMessageTests
    {

        [TestMethod]
        public void TestEventMessage()
        {
            var stm = new MemoryStream();
            var wrt = new RnetStreamWriter(stm);

            var msg = new RnetEventMessage(
                RnetDeviceId.RootController,
                RnetDeviceId.External,
                new RnetPath(2, 0),
                null,
                RnetEvent.VolumeUp,
                0,
                0,
                1);

            msg.Write(wrt);

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
