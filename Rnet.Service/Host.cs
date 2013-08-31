using System;
using System.Diagnostics.Contracts;
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
        Uri uri = new Uri("rnet.tcp://tokyo.cogito.cx:9999");
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
            if (bus == null)
            {
                bus = new RnetBus(uri);
                await bus.Start();
            }

            if (deviceHost == null)
            {
                deviceHost = new WebServiceHost(new Devices.DeviceService(bus), new Uri("http://localhost:12292/rnet/devices/"));
                deviceHost.Open();
            }

            if (objectHost == null)
            {
                objectHost = new WebServiceHost(new Objects.ObjectService(bus), new Uri("http://localhost:12292/rnet/objects/"));
                objectHost.Open();
            }
        }

        public void OnStop()
        {
            Contract.Assert(sync != null);

            sync.Post(i => OnStopAsync(), null);
            sync.Stop();
        }

        /// <summary>
        /// Stops the service, from within synchronization context.
        /// </summary>
        async void OnStopAsync()
        {
            if (deviceHost != null)
            {
                deviceHost.Close();
                deviceHost = null;
            }

            if (objectHost != null)
            {
                objectHost.Close();
                objectHost = null;
            }

            // allow bus to stay alive, just shut it down
            await bus.Stop();
        }

    }

}
