using System;
using System.Reflection;
using System.Windows;

namespace Rnet.Manager
{

    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            Assembly.Load("System.Windows.Interactivity");
            Rnet.Drivers.Russound.DriverPackage.Register();

            base.OnStartup(e);
        }

    }

}
