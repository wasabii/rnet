using System;
using System.ServiceProcess;

namespace Rnet.Service
{

    public static class Program
    {

        public static void Main()
        {
#if DEBUG
            var h = new Host();
            h.OnStart();
            Console.ReadLine();
            h.OnStop();
            Console.ReadLine();
#else
            ServiceBase.Run(new[] { new RnetService() });
#endif
        }

    }

}
