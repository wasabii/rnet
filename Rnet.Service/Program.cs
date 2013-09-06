using System;
using System.Diagnostics;
using Rnet.Service.Host.Host;

namespace Rnet.Service
{

    public class Program
    {

        static readony TraceSource source = new TraceSource("Rnet.Service", SourceLevels.All);

        /// <summary>
        /// Main program entry point.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            using (var s = new ServiceImpl())
            {
                Trace.Write
                s.Start();
                Console.ReadLine();
                s.Stop();
            }
        }

        /// <summary>
        /// Main instance entry point.
        /// </summary>
        /// <param name="args"></param>
        void Run(string[] args)
        {
            web = new RnetHost()
            web.Start();
#if DEBUG
            Console.ReadLine();
            web.Stop();
            Console.ReadLine();
#else
            ServiceBase.Run(new[] { new RnetService() });
#endif
        }

    }

}
