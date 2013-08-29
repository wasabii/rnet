using System;
using System.Windows;

namespace Rnet.Manager
{

    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            Rnet.Drivers.Russound.DriverPackage.Register();

            base.OnStartup(e);
        }

    }

}
