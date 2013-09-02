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

            Get["/"] = x => GetDevices();
            Get["/{ControllerId}/{*Uri}"] = x => BadControllerPath(x.ControllerId, x.Uri);
            Get["/{ControllerId}.{ZoneId}.{KeypadId}"] = x => GetDevice(x.ControllerId, x.ZoneId, x.KeypadId);
            Get["/{ControllerId}.{ZoneId}.{KeypadId}/data/{*Path}"] = x => GetDeviceData(x.ControllerId, x.ZoneId, x.KeypadId, x.Path);
            Put["/{ControllerId}.{ZoneId}.{KeypadId}/data/{*Path}", true] = (x, ct) => PutDeviceData(Request.Body, x.ControllerId, x.ZoneId, x.KeypadId, x.Path);
        }

        /// <summary>
        /// Gets all the available devices.
        /// </summary>
        /// <returns></returns>
        DeviceCollection GetDevices()
        {
            return new DeviceCollection(Bus.Controllers
                .SelectMany(i => i.Zones)
                .SelectMany(i => i.Devices)
                .Cast<RnetDevice>()
                .Concat(Bus.Controllers)
                .OrderBy(i => i.DeviceId)
                .Select(i => RnetDeviceToInfo(i)));
        }

        /// <summary>
        /// Gets the controller descriptor.
        /// </summary>
        /// <param name="controllerId"></param>
        /// <returns></returns>
        dynamic BadControllerPath(string controllerId, string uri)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(controllerId));
            Contract.Requires<ArgumentNullException>(uri != null);

            var controller = Bus.Controllers[int.Parse(controllerId)];
            if (controller == null)
                throw new HttpException(HttpStatusCode.NotFound);

            return Response.AsRedirect(new Uri(GetDeviceUri(controller), uri).ToString(), RedirectResponse.RedirectType.Permanent);
        }

        /// <summary>
        /// Gets the device description.
        /// </summary>
        /// <param name="controllerId"></param>
        /// <param name="zoneId"></param>
        /// <param name="keypadId"></param>
        /// <returns></returns>
        Device GetDevice(string controllerId, string zoneId, string keypadId)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(controllerId));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(zoneId));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(keypadId));

            return RnetDeviceToInfo(GetRnetDevice(controllerId, zoneId, keypadId));
        }

        /// <summary>
        /// Gets the device data.
        /// </summary>
        /// <param name="controllerId"></param>
        /// <param name="zoneId"></param>
        /// <param name="keypadId"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        async Task<Response> GetDeviceData(string controllerId, string zoneId, string keypadId, string path)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(controllerId));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(zoneId));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(keypadId));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path));

            return await GetDeviceData(GetRnetDevice(controllerId, zoneId, keypadId), path);
        }

        /// <summary>
        /// Updates device data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="controllerId"></param>
        /// <param name="zoneId"></param>
        /// <param name="keypadId"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        async Task PutDeviceData(Stream data, string controllerId, string zoneId, string keypadId, string path)
        {
            Contract.Requires<ArgumentNullException>(data != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(controllerId));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(zoneId));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(keypadId));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path));

            await PutDeviceData(GetRnetDevice(controllerId, zoneId, keypadId), path, data);
        }

        /// <summary>
        /// Gets the <see cref="IRnetZoneDevice"/> given by the IDs.
        /// </summary>
        /// <param name="controllerId"></param>
        /// <param name="zoneId"></param>
        /// <param name="keypadId"></param>
        /// <returns></returns>
        RnetDevice GetRnetDevice(string controllerId, string zoneId, string keypadId)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(controllerId));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(zoneId));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(keypadId));

            return Bus[byte.Parse(controllerId), byte.Parse(zoneId), byte.Parse(keypadId)];
        }

        /// <summary>
        /// Gets the data from the given path of the device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        async Task<Response> GetDeviceData(RnetDevice device, string path)
        {
            Contract.Requires<ArgumentNullException>(device != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path));

            var handle = device[RnetPath.Parse(path.Replace('/', '.'))];
            if (handle == null)
                throw new HttpException(HttpStatusCode.NotFound);

            //// check for cache lifetime
            //IncomingRequest.CheckConditionalRetrieve(handle.Timestamp);

            //// refresh if requested
            //CacheControlHeaderValue cc;
            //if (CacheControlHeaderValue.TryParse(IncomingRequest.Headers[HttpRequestHeader.CacheControl], out cc))
            //    if (cc.MustRevalidate)
            //        await handle.Refresh();

            // read data
            var data = await handle.Read();
            if (data == null)
                throw new HttpException(HttpStatusCode.NotFound);

            return Response.FromStream(new MemoryStream(data), "application/octet-stream")
                .WithHeader("Last-Modified", handle.Timestamp.ToString("R"));
        }

        /// <summary>
        /// Puts the given data into the given path of the device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        async Task PutDeviceData(RnetDevice device, string path, Stream data)
        {
            Contract.Requires<ArgumentNullException>(device != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path));
            Contract.Requires<ArgumentNullException>(data != null);

            var handle = device[RnetPath.Parse(path.Replace('/', '.'))];
            if (handle == null)
                throw new HttpException(HttpStatusCode.NotFound);

            await handle.Write(data);
        }

    }

}
