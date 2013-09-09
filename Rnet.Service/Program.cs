using System;

namespace Rnet.Service
{

    public class Program
    {

        static readonly ServiceImpl impl;

        /// <summary>
        /// Main program entry point.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            using (var s = new ServiceImpl())
            {
                Console.WriteLine("Starting ...");
                s.OnStart(args);
                Console.WriteLine("Started ...");

                Console.ReadLine();

                Console.WriteLine("Stopping ...");
                s.OnStop();
                Console.WriteLine("Stopped ...");

                Console.WriteLine("Press any key to exit.");
                Console.ReadLine();
            }
        }

    }

}
