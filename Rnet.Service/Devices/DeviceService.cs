using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading.Tasks;

namespace Rnet.Service.Devices
{

    [ServiceContract(Namespace = "urn:rnet:service", Name = "devices")]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, UseSynchronizationContext = true)]
    [FormatServiceBehavior]
    class DeviceService : WebServiceBase
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="bus"></param>
        internal DeviceService(RnetBus bus)
            : base(bus)
        {
            Contract.Requires<ArgumentNullException>(bus != null);
        }

        /// <summary>
        /// Gets all the available devices.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "")]
        public DeviceCollection GetDevices()
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
        [OperationContract]
        [WebGet(UriTemplate = "{controllerId}/{*uri}")]
        public void BadControllerPath(string controllerId, string uri)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(controllerId));
            Contract.Requires<ArgumentNullException>(uri != null);

            var controller = Bus.Controllers[int.Parse(controllerId)];
            if (controller == null)
                throw new WebFaultException(HttpStatusCode.NotFound);

            // redirect to proper url
            OutgoingResponse.Location = new Uri(GetDeviceUri(controller), uri).ToString();
            OutgoingResponse.StatusCode = HttpStatusCode.RedirectKeepVerb;
        }

        /// <summary>
        /// Gets the device description.
        /// </summary>
        /// <param name="controllerId"></param>
        /// <param name="zoneId"></param>
        /// <param name="keypadId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "{controllerId}.{zoneId}.{keypadId}")]
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
        [OperationContract]
        [WebGet(UriTemplate = "{controllerId}.{zoneId}.{keypadId}/data/{*path}")]
        public async Task<byte[]> GetDeviceData(string controllerId, string zoneId, string keypadId, string path)
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
        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "{controllerId}.{zoneId}.{keypadId}/data/{*path}")]
        public async Task PutDeviceData(byte[] data, string controllerId, string zoneId, string keypadId, string path)
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

            var controller = Bus.Controllers[int.Parse(controllerId)];
            if (controller == null)
                throw new WebFaultException(HttpStatusCode.NotFound);

            var zone = controller.Zones[int.Parse(zoneId)];
            if (zone == null)
                throw new WebFaultException(HttpStatusCode.NotFound);

            var device = zone.Devices[int.Parse(keypadId)] as RnetZoneRemoteDevice;
            if (device == null)
                throw new WebFaultException(HttpStatusCode.NotFound);

            return device;
        }

        /// <summary>
        /// Gets the data from the given path of the device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        async Task<byte[]> GetDeviceData(RnetDevice device, string path)
        {
            Contract.Requires<ArgumentNullException>(device != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path));

            var handle = device[RnetPath.Parse(path.Replace('/', '.'))];
            if (handle == null)
                throw new WebFaultException(HttpStatusCode.NotFound);

            // check for cache lifetime
            IncomingRequest.CheckConditionalRetrieve(handle.Timestamp);

            // refresh if requested
            CacheControlHeaderValue cc;
            if (CacheControlHeaderValue.TryParse(IncomingRequest.Headers[HttpRequestHeader.CacheControl], out cc))
                if (cc.MustRevalidate)
                    await handle.Refresh();

            // read data
            var data = await handle.Read();
            if (data == null)
                throw new WebFaultException(HttpStatusCode.NotFound);

            OutgoingResponse.LastModified = handle.Timestamp;
            return data;
        }

        /// <summary>
        /// Puts the given data into the given path of the device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        async Task PutDeviceData(RnetDevice device, string path, byte[] data)
        {
            Contract.Requires<ArgumentNullException>(device != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path));
            Contract.Requires<ArgumentNullException>(data != null);

            var handle = device[RnetPath.Parse(path.Replace('/', '.'))];
            if (handle == null)
                throw new WebFaultException(HttpStatusCode.NotFound);

            await handle.Write(data);
        }

    }

}
