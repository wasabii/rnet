using System;
using System.ServiceModel.Web;

namespace Rnet.Service
{

    class ServiceHost
    {

        static ServiceHost()
        {
            Rnet.Drivers.Russound.DriverPackage.Register();
        }

        SingleThreadSynchronizationContext sync = new SingleThreadSynchronizationContext();
        Uri uri = new Uri("rnet.tcp://tokyo.cogito.cx:9999");
        RnetBus bus;
        WebServiceHost host;

        public void OnStart()
        {
            sync.Post(i => OnStartAsync(), null);
            sync.Start();
        }

        /// <summary>
        /// Starts the service, from within synchronization context.
        /// </summary>
        async void OnStartAsync()
        {
            bus = new RnetBus(uri);
            await bus.Start();

            host = new WebServiceHost(new RnetServiceImplementation(bus), new Uri("http://localhost:12292/rnet/"));
            host.Open();
        }

        public void OnStop()
        {
            sync.Post(i => OnStopAsync(), null);
            sync.Complete();
        }

        /// <summary>
        /// Stops the service, from within synchronization context.
        /// </summary>
        async void OnStopAsync()
        {
            host.Close();

            await bus.Stop();
        }

    }

}
