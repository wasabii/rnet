using System;

using Rnet.Protocol;

namespace Rnet.Monitor
{

    public static class RnetClientTest
    {

        static RnetClient c;

        public static void Main(string[] args)
        {
            c = new RnetClient(new RnetTcpConnection("tokyo.larvalstage.net", 9999));
            c.StateChanged += c_StateChanged;
            c.ConnectionStateChanged += c_ConnectionStateChanged;
            c.MessageReceived += c_MessageReceived;
            c.Error += c_Error;
            c.Start();

            Console.ReadLine();
        }

        static void c_StateChanged(object sender, RnetClientStateEventArgs args)
        {
            Console.WriteLine(args.State);
        }

        static void c_ConnectionStateChanged(object sender, RnetConnectionStateEventArgs args)
        {
            Console.WriteLine(args.State);
        }

        static void c_MessageReceived(object sender, RnetMessageEventArgs args)
        {
            Console.WriteLine(args.Message);
        }

        static void c_Error(object sender, RnetClientErrorEventArgs args)
        {
            Console.WriteLine(args.Exception);
        }

    }

}
