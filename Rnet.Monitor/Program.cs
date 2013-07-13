using System;
using System.Threading;

namespace Rnet.Monitor
{

    public static class Program
    {

        static RnetConnection rnet;
        static CancellationTokenSource cts = new CancellationTokenSource();

        public static void Main(string[] args)
        {
            using (rnet = new RnetTcpConnection("tokyo.larvalstage.net", 9999))
            {
                rnet.Open();

                var t = new Thread(ReadThreadMain);
                t.Start();

                var m1 = new RnetEventMessage(
                    RnetDeviceId.RootController,
                    RnetDeviceId.External,
                    new RnetPath(0x02, 0x00),
                    new RnetPath(),
                    RnetEvents.SetZoneVolume,
                    (ushort)0x12,
                    0x00,
                    0x01);
                rnet.Send(m1);

                var m2 = new RnetRequestDataMessage(
                    RnetDeviceId.RootController,
                    RnetDeviceId.External,
                    new RnetPath(0x02, 0x00, 0x00, 0x07),
                    new RnetPath(),
                    RnetRequestMessageType.Data);
                rnet.Send(m2);

                Console.ReadLine();
                cts.Cancel();
                t.Join();
            }
        }

        static void ReadThreadMain()
        {
            while (!cts.IsCancellationRequested)
                Console.WriteLine(rnet.Receive().DebugView);
        }

    }

}
