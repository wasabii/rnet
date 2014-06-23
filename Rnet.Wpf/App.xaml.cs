using System;
using System.Composition.Hosting;
using System.Threading.Tasks;
using System.Windows;
using Rnet.Client;

namespace Rnet.Wpf
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App :
        Application
    {

        static readonly Uri uri = new Uri("http://kyoto.cogito.cx:12292/rnet/");

        public App()
        {
            var c = new ContainerConfiguration()
                .WithAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                .WithAssembly(typeof(RnetClient).Assembly)
                .CreateContainer();

            Task.Run(async () =>
            {
                var p = c.GetExport<IRnetObjectProvider>();
                var o = await p.GetAsync(uri);
                Console.WriteLine(o);
            });
        }

    }

}
