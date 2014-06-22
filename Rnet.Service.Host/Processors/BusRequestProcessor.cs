using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.Owin;

using Rnet.Service.Host.Models;

namespace Rnet.Service.Host.Processors
{

    /// <summary>
    /// Handles requests against a <see cref="RnetBus"/>.
    /// </summary>
    [RequestProcessor(typeof(RnetBus))]
    public class BusRequestProcessor :
        RequestProcessor<RnetBus>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        [ImportingConstructor]
        protected BusRequestProcessor(
            RootRequestProcessor module)
            : base(module)
        {
            Contract.Requires<ArgumentNullException>(module != null);
        }

        public override async Task<object> Resolve(IOwinContext context, RnetBus bus, string[] path)
        {
            // if bus is down, so are we
            if (bus.State != RnetBusState.Started ||
                bus.Client.State != RnetClientState.Started ||
                bus.Client.Connection.State != RnetConnectionState.Open)
                return HttpStatusCode.ServiceUnavailable;

            // path represents a direct device ID
            if (path[0][0] == ':')
                return ResolveDevice(context, bus, path, path[0].Substring(1));

            return await ResolveObject(context, bus, path);
        }

        /// <summary>
        /// Resolves a device ID into a device.
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        object ResolveDevice(IOwinContext context, RnetBus bus, string[] path, string deviceId)
        {
            Contract.Requires<ArgumentNullException>(bus != null);
            Contract.Requires<ArgumentNullException>(path != null);
            Contract.Requires<ArgumentNullException>(deviceId != null);

            // return device
            return new ResolveResponse(bus[ParseRnetDeviceId(deviceId)], path.Skip(1).ToArray());
        }

        /// <summary>
        /// Parses the string into a <see cref="RnetDeviceId"/>.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        RnetDeviceId ParseRnetDeviceId(string t)
        {
            Contract.Requires<ArgumentNullException>(t != null);

            var s = t.Split('.');

            // single number: controller
            if (s.Length == 1)
                return new RnetDeviceId(
                    byte.Parse(s[0]),
                    RnetZoneId.Zone1,
                    RnetKeypadId.Controller);

            // standard format
            if (s.Length == 3)
                return new RnetDeviceId(
                    byte.Parse(s[0]),
                    byte.Parse(s[1]),
                    byte.Parse(s[2]));

            throw new FormatException("RnetDeviceId");
        }

        /// <summary>
        /// Resolves a object.
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        async Task<object> ResolveObject(IOwinContext context, RnetBus bus, string[] path)
        {
            Contract.Requires<ArgumentNullException>(bus != null);
            Contract.Requires<ArgumentException>(path != null && path.Length > 0);

            // path represents a root object (controller, for now)
            var o = await Module.FindObject(bus.Controllers, path[0]);
            if (o != null)
                return new ResolveResponse(o, path.Skip(1).ToArray());

            return null;
        }

        public override async Task<object> Get(IOwinContext context, RnetBus bus)
        {
            // all devices available on the bus
            var l = await Task.WhenAll(Enumerable.Empty<RnetBusObject>()
                .Concat(bus.Controllers)
                .Concat(bus.Controllers.SelectMany(i => i.Zones).SelectMany(i => i.Devices))
                .OfType<RnetDevice>()
                .Select(i => Module.ObjectToData(context, i)));

            return new BusData()
            {
                Uri = context.GetBaseUri(),
                Devices = new DeviceDataCollection(l.OfType<DeviceData>()),
                Objects = new ObjectDataCollection(l.OfType<ObjectData>()),
            };
        }

    }

}
