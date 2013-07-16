using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rnet.Protocol;

namespace Rnet.Protocol.Tests
{

    [TestClass]
    public class RnetHandshakeMessageTests
    {

        [TestMethod]
        public void TestHandshakeMessage()
        {
            var stm = new MemoryStream();
            var wrt = new RnetStreamWriter(stm);

            var msg = new RnetHandshakeMessage(
                RnetDeviceId.RootController,
                RnetDeviceId.External,
                RnetHandshakeType.Data);

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
                0x02, 
                0x02, 
                0x6C, 
                0xF7,
            };

            var actual = stm.ToArray();

            Assert.IsTrue(actual.SequenceEqual(expected));
        }

    }

}
