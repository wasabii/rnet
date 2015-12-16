using Topshelf;
using Topshelf.ServiceConfigurators;

namespace Rnet.Service
{

    public class Program
    {

        /// <summary>
        /// Main program entry point.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.SetDisplayName("Rnet");
                x.SetDescription("Rnet sevice bus implementation");
                x.Service<ServiceImpl>((ServiceConfigurator<ServiceImpl> s) =>
                {
                    s.ConstructUsing(() => new ServiceImpl());
                    s.WhenStarted(tc => tc.OnStart(args));
                    s.WhenStopped(tc => tc.OnStop());
                });
            });
        }

    }

}
