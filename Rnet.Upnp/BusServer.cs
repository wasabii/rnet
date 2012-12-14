using System;

using Mono.Upnp;

namespace Rnet.Upnp
{

    public class BusServer
    {

        public static readonly DeviceType DeviceType = new DeviceType("tempuri-org", "RNetBus", new Version(1, 0));

        Server server;
        Bus bus;

        public BusServer(string udn, string friendlyName, string manufacturer, string modelName, Bus bus)
        {
            server = new Server(new Root(DeviceType, udn, friendlyName, manufacturer, modelName,
                new DeviceOptions()
                {
                    Services = new[]
                    {
                        new Service<Bus>(Bus.ServiceType, "urn:tempuri-org:serviceId:Bus", this.bus = bus),
                    },
                }));
        }

        public void Start()
        {
            server.Start();
        }

        public void Stop()
        {
            server.Stop();
        }

    }

}
