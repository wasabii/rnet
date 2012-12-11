using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Threading;

namespace Rnet.Tests
{

    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void TestMessage1()
        {
            var stm = new MemoryStream();

            // expected volumn up message
            var org = new byte[] {
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

            using (var rnet = new RnetStreamConnection(stm))
            {
                rnet.Open();

                // message without start/end/checksum
                rnet.WriteMessage(
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
            }

            Assert.IsTrue(stm.ToArray().SequenceEqual(org));
        }

        [TestMethod]
        public void TestInvertMessage1()
        {
            var stm = new MemoryStream();

            // expected volumn down message
            var org = new byte[] {
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
                0x80,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x01,
                0x7B,
                0xF7,
            };

            using (var rnet = new RnetStreamConnection(stm))
            {
                rnet.Open();

                // message without start/end/checksum
                rnet.WriteMessage(
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
            }

            Assert.IsTrue(stm.ToArray().SequenceEqual(org));
        }

        [TestMethod]
        public void TestRnet()
        {
            using (var rnet = new RnetTcpConnection("tokyo.larvalstage.net", 9999))
            {
                rnet.Open();

                // message without start/end/checksum
                for (int i = 0; i <= 50; i++)
                {
                    rnet.WriteMessage(
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
                        0x80,
                        0x00,
                        0x00,
                        0x00,
                        0x00,
                        0x00,
                        0x01);
                    Thread.Sleep(100);
                }

                // message without start/end/checksum
                for (int i = 0; i <= 50; i++)
                {
                    rnet.WriteMessage(
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
                    Thread.Sleep(100);
                }
            }
        }

    }

}
