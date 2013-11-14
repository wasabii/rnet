using System.Reflection;
using System.Windows;

namespace Rnet.Manager
{

    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs args)
        {
            Assembly.Load("System.Windows.Interactivity");
            base.OnStartup(args);
        }

    }

}
