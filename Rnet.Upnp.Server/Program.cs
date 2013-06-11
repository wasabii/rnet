using System;
using System.Threading.Tasks;

namespace Rnet.Upnp.Server
{

    public static class Program
    {

        public static void Main(string[] args)
        {
            Task.Run(() =>
            {
                var bus = new Bus();
                var srv = new BusServer("uuid:" + Guid.NewGuid(), "RNet Bus", "Russound", "CAM6.6", bus);
                srv.Start();
            }).Wait();

            Console.ReadLine();
        }

    }

}
