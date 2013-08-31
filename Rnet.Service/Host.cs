using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Rnet.Service
{

    class Host
    {

        /// <summary>
        /// Initializes the static instance.
        /// </summary>
        static Host()
        {
            Rnet.Drivers.Russound.DriverPackage.Register();
        }

        SingleThreadSynchronizationContext sync = new SingleThreadSynchronizationContext();
        Uri uri = new Uri("rnet.tcp://173.175.230.163:9999");
        RnetBus bus;
        WebServiceHost deviceHost;
        WebServiceHost objectHost;

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

            deviceHost = new WebServiceHost(new Devices.DeviceService(bus), new Uri("http://localhost:12292/rnet/devices/"));
            deviceHost.Open();

            objectHost = new WebServiceHost(new Objects.ObjectService(bus), new Uri("http://localhost:12292/rnet/objects/"));
            objectHost.Open();
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
            deviceHost.Close();
            objectHost.Close();

            await bus.Stop();
        }

    }

}
