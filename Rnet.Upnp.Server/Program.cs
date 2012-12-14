using System;

namespace Rnet.Upnp.Server
{

    public static class Program
    {

        public static void Main(string[] args)
        {
            var bus = new Bus();
            var srv = new BusServer("uuid:" + Guid.NewGuid(), "RNet Bus", "Russound", "CAM6.6", bus);
            srv.Start();

            Console.ReadLine();
        }

    }

}
