using System;
using System.ServiceModel;
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
        WebServiceHost busHost;
        WebServiceHost objHost;

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

            busHost = new WebServiceHost(new Devices.DeviceService(bus), new Uri("http://localhost:12292/rnet/devices/"));
            busHost.AddServiceEndpoint(typeof(Devices.DeviceService), new WebHttpBinding(), "");
            busHost.Open();

            objHost = new WebServiceHost(new Objects.ObjectService(bus), new Uri("http://localhost:12292/rnet/objects/"));
            objHost.AddServiceEndpoint(typeof(Objects.ObjectService), new WebHttpBinding(), "");
            objHost.Open();
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
            busHost.Close();
            objHost.Close();

            await bus.Stop();
        }

    }

}
