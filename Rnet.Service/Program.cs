using System;
using System.ServiceProcess;

namespace Rnet.Service
{

    public static class Program
    {

        public static void Main()
        {
#if DEBUG
            var h = new RnetHost();
            h.Start();
            Console.ReadLine();
            h.Stop();
            Console.ReadLine();
#else
            ServiceBase.Run(new[] { new RnetService() });
#endif
        }

    }

}
