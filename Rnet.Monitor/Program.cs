using System;

namespace Rnet.Monitor
{

    public static class Program
    {

        public static void Main(string[] args)
        {
            using (var rnet = new RnetTcpConnection("tokyo.larvalstage.net", 9999))
            {
                rnet.Open();
                rnet.MessageReceived += rnet_MessageReceived;

                var m1 = new RnetEventMessage(
                     new RnetDeviceId(0x00, 0x00, RnetKeypadId.Controller),
                     new RnetDeviceId(0x00, 0x00, 0x70),
                     new RnetPath(0x02, 0x00),
                     new RnetPath(),
                     RnetEvents.SetZoneVolume,
                     (ushort)0x12,
                     0x00,
                     0x01);
                rnet.Send(m1);

                var m2 = new RnetRequestDataMessage(
                    new RnetDeviceId(0x00, 0x00, 0x7f),
                    new RnetDeviceId(0x00, 0x00, 0x70),
                    new RnetPath(0x02, 0x00, 0x00, 0x06),
                    new RnetPath());
                rnet.Send(m2);

                Console.ReadLine();
            }
        }

        static void rnet_MessageReceived(object sender, RnetMessageReceivedEventArgs args)
        {
            Console.WriteLine("RECEIVED: {0}", args.Message.DebugView);
            Console.WriteLine();
        }

    }

}
