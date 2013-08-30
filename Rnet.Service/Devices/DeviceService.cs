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

    [ServiceContract]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, UseSynchronizationContext = true)]
    class DeviceService : WebServiceBase
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="bus"></param>
        internal DeviceService(RnetBus bus)
            : base(bus)
        {

        }

        static ControllerRef ControllerToRef(RnetController controller)
        {
            Contract.Requires<ArgumentNullException>(controller != null);

            return new ControllerRef()
            {
                Id = new Uri(BaseUri, string.Format("{0}", (int)controller.Id)),
            };
        }

        static Controller ControllerToInfo(RnetController controller)
        {
            Contract.Requires<ArgumentNullException>(controller != null);

            return new Controller()
            {
                Id = new Uri(BaseUri, string.Format("{0}", (int)controller.Id)),
                DeviceId = DeviceIdToString(controller.DeviceId),
                Zones = controller.Zones
                    .Select(zone => ZoneToRef(zone))
                    .ToList(),
            };
        }

        static ZoneRef ZoneToRef(RnetZone zone)
        {
            Contract.Requires<ArgumentNullException>(zone != null);

            return new ZoneRef()
            {
                Id = new Uri(BaseUri, string.Format("{0}/{1}", (int)zone.Controller.Id, (int)zone.Id)),
            };
        }

        static Zone ZoneToInfo(RnetZone zone)
        {
            Contract.Requires<ArgumentNullException>(zone != null);

            return new Zone()
            {
                Id = new Uri(BaseUri, string.Format("{0}/{1}", (int)zone.Controller.Id, (int)zone.Id)),
                Controller = ControllerToRef(zone.Controller),
                Devices = zone.Devices
                    .OfType<RnetZoneRemoteDevice>()
                    .Select(device => DeviceToRef(device))
                    .ToList(),
            };
        }

        static DeviceRef DeviceToRef(RnetZoneRemoteDevice device)
        {
            Contract.Requires<ArgumentNullException>(device != null);

            return new DeviceRef()
            {
                Id = new Uri(BaseUri, string.Format("{0}/{1}/{2}", (int)device.DeviceId.ControllerId, (int)device.DeviceId.ZoneId, (int)device.DeviceId.KeypadId)),
            };
        }

        static Device DeviceToInfo(RnetZoneRemoteDevice device)
        {
            Contract.Requires<ArgumentNullException>(device != null);

            return new Device()
            {
                Id = new Uri(BaseUri, string.Format("{0}/{1}/{2}", (int)device.DeviceId.ControllerId, (int)device.DeviceId.ZoneId, (int)device.DeviceId.KeypadId)),
                DeviceId = DeviceIdToString(device.DeviceId),
                Zone = ZoneToRef(device.Zone),
                Controller = ControllerToRef(device.Zone.Controller),
            };
        }

        /// <summary>
        /// Gets the bus descriptor.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "")]
        public Bus GetBus()
        {
            return new Bus()
            {
                Controllers = Bus.Controllers
                    .Select(controller => ControllerToRef(controller))
                    .ToList(),
            };
        }

        /// <summary>
        /// Gets the controller descriptor.
        /// </summary>
        /// <param name="controllerId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "{controllerId}")]
        public Controller GetController(string controllerId)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(controllerId));

            var controller = Bus.Controllers[int.Parse(controllerId)];
            if (controller == null)
                throw new WebFaultException(HttpStatusCode.NotFound);

            return ControllerToInfo(controller);
        }

        /// <summary>
        /// Gets controller data.
        /// </summary>
        /// <param name="controllerId"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "{controllerId}/data/{*path}")]
        public async Task<byte[]> GetControllerData(string controllerId, string path)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(controllerId));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path));

            var controller = Bus.Controllers[int.Parse(controllerId)];
            if (controller == null)
                throw new WebFaultException(HttpStatusCode.NotFound);

            return await GetDeviceData(controller, path);
        }

        /// <summary>
        /// Updates controller data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="controllerId"></param>
        /// <param name="path"></param>
        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "{controllerId}/data/{*path}")]
        public async void PutControllerData(byte[] data, string controllerId, string path)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(controllerId));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path));

            var controller = Bus.Controllers[int.Parse(controllerId)];
            if (controller == null)
                throw new WebFaultException(HttpStatusCode.NotFound);

            await PutDeviceData(controller, path, data);
        }

        /// <summary>
        /// Gets the zone description.
        /// </summary>
        /// <param name="controllerId"></param>
        /// <param name="zoneId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "{controllerId}/{zoneId}")]
        public Zone GetZone(string controllerId, string zoneId)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(controllerId));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(zoneId));

            var controller = Bus.Controllers[int.Parse(controllerId)];
            if (controller == null)
                throw new WebFaultException(HttpStatusCode.NotFound);

            var zone = controller.Zones[int.Parse(zoneId)];
            if (zone == null)
                throw new WebFaultException(HttpStatusCode.NotFound);

            return ZoneToInfo(zone);
        }

        /// <summary>
        /// Gets the device description.
        /// </summary>
        /// <param name="controllerId"></param>
        /// <param name="zoneId"></param>
        /// <param name="keypadId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "{controllerId}/{zoneId}/{keypadId}")]
        Device GetDevice(string controllerId, string zoneId, string keypadId)
        {
            return DeviceToInfo(GetRnetDevice(controllerId, zoneId, keypadId));
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
        [WebGet(UriTemplate = "{controllerId}/{zoneId}/{keypadId}/data/{*path}")]
        public async Task<byte[]> GetDeviceData(string controllerId, string zoneId, string keypadId, string path)
        {
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
        [WebInvoke(Method = "PUT", UriTemplate = "{controllerId}/{zoneId}/{keypadId}/data/{*path}")]
        public async Task PutDeviceData(byte[] data, string controllerId, string zoneId, string keypadId, string path)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path));
            await PutDeviceData(GetRnetDevice(controllerId, zoneId, keypadId), path, data);
        }

        /// <summary>
        /// Gets the <see cref="RnetZoneRemoteDevice"/> given by the IDs.
        /// </summary>
        /// <param name="controllerId"></param>
        /// <param name="zoneId"></param>
        /// <param name="keypadId"></param>
        /// <returns></returns>
        RnetZoneRemoteDevice GetRnetDevice(string controllerId, string zoneId, string keypadId)
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

            WebOperationContext.Current.OutgoingResponse.LastModified = handle.Timestamp;
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
            var handle = device[RnetPath.Parse(path.Replace('/', '.'))];
            if (handle == null)
                throw new WebFaultException(HttpStatusCode.NotFound);

            await handle.Write(data);
        }

    }

}
