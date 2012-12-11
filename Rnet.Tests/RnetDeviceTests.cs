using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rnet.Tests
{

    [TestClass]
    public class RnetDeviceTests
    {

        [TestMethod]
        public void TestVolume()
        {
            using (var rnet = new RnetTcpConnection("tokyo.larvalstage.net", 9999))
            {
                rnet.Open();

                for (int t = 0; t < 10; t++)
                {
                    for (int i = 0; i <= 50; i++)
                    {
                        rnet.Send(
                            new RnetEventMessage(
                                RnetDeviceId.RootControllerTarget,
                                RnetDeviceId.ExternalSource,
                                new RnetPath(2).Next(0),
                                null,
                                RnetEvents.VolumeDown,
                                0,
                                0,
                                1));
                        Thread.Sleep(50);
                    }

                    Thread.Sleep(2000);

                    for (int i = 0; i <= 50; i++)
                    {
                        rnet.Send(
                            new RnetEventMessage(
                                RnetDeviceId.RootControllerTarget,
                                RnetDeviceId.ExternalSource,
                                new RnetPath(2).Next(0),
                                null,
                                RnetEvents.VolumeUp,
                                0,
                                0,
                                1));
                        Thread.Sleep(50);
                    }
                }
            }

        }

    }

}
