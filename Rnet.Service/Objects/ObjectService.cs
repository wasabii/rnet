using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading.Tasks;

using Rnet.Drivers;
using Rnet.Service.Devices;

namespace Rnet.Service.Objects
{

    [ServiceContract]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, UseSynchronizationContext = true)]
    class ObjectService
    {

        RnetBus bus;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="bus"></param>
        internal ObjectService(RnetBus bus)
        {
            this.bus = bus;
        }

        /// <summary>
        /// Gets the base URI of the current request.
        /// </summary>
        /// <returns></returns>
        Uri BaseUri
        {
            get { return WebOperationContext.Current.IncomingRequest.UriTemplateMatch.BaseUri; }
        }

        /// <summary>
        /// Converts the given <see cref="RnetDeviceId"/> into a string.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string DeviceIdToString(RnetDeviceId id)
        {
            return string.Format("{0}.{1}.{2}", (int)id.ControllerId, (int)id.ZoneId, (int)id.KeypadId);
        }

        ControllerRef ControllerToRef(RnetController controller)
        {
            Contract.Requires<ArgumentNullException>(controller != null);

            return new ControllerRef()
            {
                Id = new Uri(BaseUri, string.Format("{0}", (int)controller.Id)),
            };
        }

        Controller ControllerToInfo(RnetController controller)
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

        ZoneRef ZoneToRef(RnetZone zone)
        {
            Contract.Requires<ArgumentNullException>(zone != null);

            return new ZoneRef()
            {
                Id = new Uri(BaseUri, string.Format("{0}/{1}", (int)zone.Controller.Id, (int)zone.Id)),
            };
        }

        Zone ZoneToInfo(RnetZone zone)
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

        DeviceRef DeviceToRef(RnetZoneRemoteDevice device)
        {
            Contract.Requires<ArgumentNullException>(device != null);

            return new DeviceRef()
            {
                Id = new Uri(BaseUri, string.Format("{0}/{1}/{2}", (int)device.DeviceId.ControllerId, (int)device.DeviceId.ZoneId, (int)device.DeviceId.KeypadId)),
            };
        }

        Device DeviceToInfo(RnetZoneRemoteDevice device)
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

        [OperationContract]
        [WebGet(UriTemplate = "/", ResponseFormat = WebMessageFormat.Json)]
        public Bus GetBus()
        {
            return new Bus()
            {
                Controllers = bus.Controllers
                    .Select(controller => ControllerToRef(controller))
                    .ToList(),
            };
        }

        [OperationContract]
        [WebGet(UriTemplate = "/{controllerId}", ResponseFormat = WebMessageFormat.Json)]
        public Controller GetController(string controllerId)
        {
            Contract.Requires<ArgumentNullException>(controllerId != null);

            var controller = bus.Controllers[int.Parse(controllerId)];
            if (controller == null)
                return null;

            return ControllerToInfo(controller);
        }

        [OperationContract]
        [WebGet(UriTemplate = "/{controllerId}/profiles", ResponseFormat = WebMessageFormat.Json)]
        public async Task<List<ProfileRef>> GetControllerProfiles(string controllerId)
        {
            Contract.Requires<ArgumentNullException>(controllerId != null);

            var controller = bus.Controllers[int.Parse(controllerId)];
            if (controller == null)
                return null;

            return (await controller.GetProfiles())
                .Select(i => new ProfileRef()
                {
                    Uri = new Uri(BaseUri, string.Format("{0}/profiles/{1}",
                        (int)controller.Id,
                        i.Metadata.Name)),
                })
                .ToList();
        }

        [OperationContract]
        [WebGet(UriTemplate = "/{controllerId}/profiles/{profileName}", ResponseFormat = WebMessageFormat.Json)]
        public ProfileInfo GetControllerProfile(string controllerId, string profileName)
        {
            throw new NotImplementedException();
        }

        [OperationContract]
        [WebGet(UriTemplate = "/{controllerId}/{zoneId}", ResponseFormat = WebMessageFormat.Json)]
        public Zone GetZone(string controllerId, string zoneId)
        {
            Contract.Requires<ArgumentNullException>(controllerId != null);
            Contract.Requires<ArgumentNullException>(zoneId != null);

            var controller = bus.Controllers[int.Parse(controllerId)];
            if (controller == null)
                return null;

            var zone = controller.Zones[int.Parse(zoneId)];
            if (zone == null)
                return null;

            return ZoneToInfo(zone);
        }

        [OperationContract]
        [WebGet(UriTemplate = "/{controllerId}/{zoneId}/{deviceId}", ResponseFormat = WebMessageFormat.Json)]
        public Device GetDevice(string controllerId, string zoneId, string deviceId)
        {
            Contract.Requires<ArgumentNullException>(controllerId != null);
            Contract.Requires<ArgumentNullException>(zoneId != null);
            Contract.Requires<ArgumentNullException>(deviceId != null);

            var controller = bus.Controllers[int.Parse(controllerId)];
            if (controller == null)
                return null;

            var zone = controller.Zones[int.Parse(zoneId)];
            if (zone == null)
                return null;

            var device = zone.Devices[int.Parse(deviceId)] as RnetZoneRemoteDevice;
            if (device == null)
                return null;

            return DeviceToInfo(device);
        }

        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = "/{controllerId}/{zoneId}/{deviceId}/profiles", ResponseFormat = WebMessageFormat.Json)]
        public async Task<List<ProfileRef>> GetDeviceProfiles(string controllerId, string zoneId, string deviceId)
        {
            Contract.Requires<ArgumentNullException>(controllerId != null);
            Contract.Requires<ArgumentNullException>(zoneId != null);
            Contract.Requires<ArgumentNullException>(deviceId != null);

            var controller = bus.Controllers[int.Parse(controllerId)];
            if (controller == null)
                return null;

            var zone = controller.Zones[int.Parse(zoneId)];
            if (zone == null)
                return null;

            var device = zone.Devices[int.Parse(deviceId)] as RnetZoneRemoteDevice;
            if (device == null)
                return null;

            return (await device.GetProfiles())
                .Select(i => new ProfileRef()
                {
                    Uri = new Uri(BaseUri, string.Format("{0}/{1}/{2}/profiles/{3}",
                        (int)device.DeviceId.ControllerId,
                        (int)device.DeviceId.ZoneId,
                        (int)device.DeviceId.KeypadId,
                        i.Metadata.Name)),
                })
                .ToList();
        }

        [OperationContract]
        [WebGet(UriTemplate = "/devices/{controllerId}/{zoneId}/{deviceId}/profiles/{profileName}", ResponseFormat = WebMessageFormat.Json)]
        public ProfileInfo GetDeviceProfile(string controllerId, string zoneId, string deviceId, string profileName)
        {
            throw new NotImplementedException();
        }

    }

}
