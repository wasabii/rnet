using System;

namespace Rnet.Monitor
{

    public static class RnetClientTest
    {

        static RnetClient c;
        static RnetBus b;

        public static void Main(string[] args)
        {
            c = new RnetClient(new RnetTcpConnection(System.Net.IPAddress.Parse("192.168.175.1"), 9999));
            c.StateChanged += c_StateChanged;
            c.ConnectionStateChanged += c_ConnectionStateChanged;
            c.MessageSent += c_MessageSent;
            c.MessageReceived += c_MessageReceived;
            c.Error += c_Error;

            b = new RnetBus(c);
            b.Start();
        }

        static void c_StateChanged(object sender, RnetClientStateEventArgs args)
        {
            Console.WriteLine(args.State);
        }

        static void c_ConnectionStateChanged(object sender, RnetConnectionStateEventArgs args)
        {
            Console.WriteLine(args.State);
        }

        static void c_MessageSent(object sender, RnetMessageEventArgs args)
        {
            Console.WriteLine("SENT");
            Console.WriteLine(args.Message);
        }

        static void c_MessageReceived(object sender, RnetMessageEventArgs args)
        {
            Console.WriteLine("RECEIVED");
            Console.WriteLine(args.Message);
        }

        static void c_Error(object sender, RnetClientErrorEventArgs args)
        {
            Console.WriteLine("ERROR");
            Console.WriteLine(args.Exception);
        }

    }

}
