using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nancy;
using Nancy.Responses;

namespace Rnet.Service.Devices
{

    [Export(typeof(INancyModule))]
    public sealed class DeviceModule : NancyModuleBase
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="bus"></param>
        [ImportingConstructor]
        public DeviceModule(RnetBus bus)
            : base(bus, "/devices")
        {
            Contract.Requires<ArgumentNullException>(bus != null);

            Get[@"/"] = x => GetDevices();
            Get[@"/(?<ControllerId>\d+)"] = x => BadControllerPath(x.ControllerId, "");
            Get[@"/(?<ControllerId>\d+)/{Uri*}"] = x => BadControllerPath(x.ControllerId, x.Uri);
            Get[@"/(?<ControllerId>\d+)\.(?<ZoneId>\d+)\.(?<KeypadId>\d+)"] = x => GetDevice(x.ControllerId, x.ZoneId, x.KeypadId);
            Get[@"/(?<ControllerId>\d+)\.(?<ZoneId>\d+)\.(?<KeypadId>\d+)/data/{Path*}"] = x => GetDeviceData(x.ControllerId, x.ZoneId, x.KeypadId, x.Path);
            Put[@"/(?<ControllerId>\d+)\.(?<ZoneId>\d+)\.(?<KeypadId>\d+)/data/{Path*}", true] = (x, ct) => PutDeviceData(Request.Body, x.ControllerId, x.ZoneId, x.KeypadId, x.Path);
        }

        /// <summary>
        /// Gets all the available devices.
        /// </summary>
        /// <returns></returns>
        dynamic GetDevices()
        {
            return new DeviceDataCollection(Bus.Controllers
                .SelectMany(i => i.Zones)
                .SelectMany(i => i.Devices)
                .Cast<RnetDevice>()
                .Concat(Bus.Controllers)
                .OrderBy(i => i.DeviceId)
                .Select(i => RnetDeviceToInfo(i)));
        }

        /// <summary>
        /// Puts the given data into the given path of the device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        async Task<dynamic> PutDeviceData(RnetDevice device, string path, Stream data)
        {
            Contract.Requires<ArgumentNullException>(device != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path));
            Contract.Requires<ArgumentNullException>(data != null);

            var handle = device[RnetPath.Parse(path.Replace('/', '.'))];
            if (handle == null)
                return HttpStatusCode.NotFound;

            return Response.FromStream(await handle.Write(data), "application/octet-stream")
                .WithHeader("Last-Modified", handle.Timestamp.ToString("R"));
        }

    }

}
