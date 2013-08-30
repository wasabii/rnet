using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Rnet.Drivers;
using Rnet.Profiles;

namespace Rnet.Service
{

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, UseSynchronizationContext = true)]
    class RnetServiceImplementation : IRnetService
    {

        RnetBus bus;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="bus"></param>
        internal RnetServiceImplementation(RnetBus bus)
        {
            this.bus = bus;
        }

        /// <summary>
        /// Gets the base URI of the current request.
        /// </summary>
        /// <returns></returns>
        Uri GetBaseUri()
        {
            return OperationContext.Current.Channel.LocalAddress.Uri;
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

        BusControllerRef ControllerToRef(RnetController controller)
        {
            Contract.Requires<ArgumentNullException>(controller != null);

            return new BusControllerRef()
            {
                Id = controller.Id,
                DeviceId = DeviceIdToString(controller.DeviceId),
                Uri = new Uri(GetBaseUri(), string.Format("devices/{0}", (int)controller.Id)),
            };
        }

        BusControllerInfo ControllerToInfo(RnetController controller)
        {
            Contract.Requires<ArgumentNullException>(controller != null);

            return new BusControllerInfo()
            {
                Id = controller.Id,
                DeviceId = DeviceIdToString(controller.DeviceId),
                Uri = new Uri(GetBaseUri(), string.Format("devices/{0}", (int)controller.Id)),
                Zones = controller.Zones
                    .Select(zone => ZoneToRef(zone))
                    .ToList(),
            };
        }

        BusZoneRef ZoneToRef(RnetZone zone)
        {
            Contract.Requires<ArgumentNullException>(zone != null);

            return new BusZoneRef()
            {
                Id = zone.Id,
                Uri = new Uri(GetBaseUri(), string.Format("devices/{0}/{1}", (int)zone.Controller.Id, (int)zone.Id)),
            };
        }

        BusZoneInfo ZoneToInfo(RnetZone zone)
        {
            Contract.Requires<ArgumentNullException>(zone != null);

            return new BusZoneInfo()
            {
                Id = zone.Controller.Id,
                Uri = new Uri(GetBaseUri(), string.Format("devices/{0}/{1}", (int)zone.Controller.Id, (int)zone.Id)),
                Controller = ControllerToRef(zone.Controller),
                Devices = zone.Devices
                    .OfType<RnetZoneRemoteDevice>()
                    .Select(device => DeviceToRef(device))
                    .ToList(),
            };
        }

        BusDeviceRef DeviceToRef(RnetZoneRemoteDevice device)
        {
            Contract.Requires<ArgumentNullException>(device != null);

            return new BusDeviceRef()
            {
                Id = device.Id,
                DeviceId = DeviceIdToString(device.DeviceId),
                Uri = new Uri(GetBaseUri(), string.Format("devices/{0}/{1}/{2}", (int)device.DeviceId.ControllerId, (int)device.DeviceId.ZoneId, (int)device.DeviceId.KeypadId)),
            };
        }

        BusDeviceInfo DeviceToInfo(RnetZoneRemoteDevice device)
        {
            Contract.Requires<ArgumentNullException>(device != null);

            return new BusDeviceInfo()
            {
                Id = (int)device.DeviceId.KeypadId,
                DeviceId = DeviceIdToString(device.DeviceId),
                Zone = ZoneToRef(device.Zone),
                Controller = ControllerToRef(device.Zone.Controller),
                Uri = new Uri(GetBaseUri(), string.Format("devices/{0}/{1}/{2}", (int)device.DeviceId.ControllerId, (int)device.DeviceId.ZoneId, (int)device.DeviceId.KeypadId)),
            };
        }

        public BusInfo GetBus()
        {
            return new BusInfo()
            {
                Controllers = bus.Controllers
                    .Select(controller => ControllerToRef(controller))
                    .ToList(),
            };
        }

        public BusControllerInfo GetController(string controllerId)
        {
            Contract.Requires<ArgumentNullException>(controllerId != null);

            var controller = bus.Controllers[int.Parse(controllerId)];
            if (controller == null)
                return null;

            return ControllerToInfo(controller);
        }

        public async Task<List<ProfileRef>> GetControllerProfiles(string controllerId)
        {
            Contract.Requires<ArgumentNullException>(controllerId != null);

            var controller = bus.Controllers[int.Parse(controllerId)];
            if (controller == null)
                return null;

            return (await controller.GetProfiles())
                .Select(i => new ProfileRef()
                {
                    Uri = new Uri(GetBaseUri(), string.Format("devices/{0}/profiles/{1}",
                        (int)controller.Id,
                        i.Metadata.Name)),
                })
                .ToList();
        }

        public ProfileInfo GetControllerProfile(string controllerId, string profileName)
        {
            throw new NotImplementedException();
        }

        public BusZoneInfo GetZone(string controllerId, string zoneId)
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

        public BusDeviceInfo GetDevice(string controllerId, string zoneId, string deviceId)
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
                    Uri = new Uri(GetBaseUri(), string.Format("devices/{0}/{1}/{2}/profiles/{3}",
                        (int)device.DeviceId.ControllerId,
                        (int)device.DeviceId.ZoneId,
                        (int)device.DeviceId.KeypadId,
                        i.Metadata.Name)),
                })
                .ToList();
        }

        public ProfileInfo GetDeviceProfile(string controllerId, string zoneId, string deviceId, string profileName)
        {
            throw new NotImplementedException();
        }

    }

}
