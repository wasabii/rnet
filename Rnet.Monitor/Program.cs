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
