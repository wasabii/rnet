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

            // configures protocol settings
            var webHttp = new RnetWebHttpBehavior()
            {
                //AutomaticFormatSelectionEnabled = true,
                //DefaultBodyStyle = WebMessageBodyStyle.Bare,
                //DefaultOutgoingResponseFormat = WebMessageFormat.Xml,
                //FaultExceptionEnabled = true,
                //HelpEnabled = true,
            };

            deviceHost = new WebServiceHost(new Devices.DeviceService(bus), new Uri("http://localhost:12292/rnet/devices/"));
            deviceHost.AddServiceEndpoint(typeof(Devices.DeviceService), new WebHttpBinding(), "")
                .EndpointBehaviors.Add(webHttp);
            deviceHost.Open();

            objectHost = new WebServiceHost(new Objects.ObjectService(bus), new Uri("http://localhost:12292/rnet/objects/"));
            objectHost.AddServiceEndpoint(typeof(Objects.ObjectService), new WebHttpBinding(), "")
                .EndpointBehaviors.Add(webHttp);
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
