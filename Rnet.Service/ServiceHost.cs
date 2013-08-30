using System;

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
        RnetWebServiceHost busHost;
        RnetWebServiceHost objHost;

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

            busHost = new RnetWebServiceHost(new Devices.DeviceService(bus), new Uri("http://localhost:12292/rnet/devices/"));
            busHost.Open();

            objHost = new RnetWebServiceHost(new Objects.ObjectService(bus), new Uri("http://localhost:12292/rnet/objects/"));
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
