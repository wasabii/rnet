using System;
using System.Diagnostics.Contracts;

using Nancy;

using Rnet.Service.Devices;

namespace Rnet.Service
{

    /// <summary>
    /// Serves as the base class for services.
    /// </summary>
    public abstract class NancyModuleBase : NancyModule
    {

        RnetBus bus;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="bus"></param>
        protected NancyModuleBase(RnetBus bus, string modulePath)
            : base(modulePath)
        {
            Contract.Requires(bus != null);

            this.bus = bus;
        }

        /// <summary>
        /// Gets the <see cref="RnetBus"/>.
        /// </summary>
        protected RnetBus Bus
        {
            get { return bus; }
        }

        /// <summary>
        /// Gets the base URI of the current request.
        /// </summary>
        /// <returns></returns>
        protected Uri BaseUri
        {
            get { return GetBaseUri(); }
        }

        /// <summary>
        /// Implements the getter for BaseUri.
        /// </summary>
        /// <returns></returns>
        Uri GetBaseUri()
        {
            var u = Request.Url.Clone();
            u.Path = ModulePath;
            return u;
        }

        /// <summary>
        /// Converts the given <see cref="RnetDeviceId"/> into a string.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected string GetDeviceIdAsString(RnetDeviceId id)
        {
            return string.Format("{0}.{1}.{2}", (int)id.ControllerId, (int)id.ZoneId, (int)id.KeypadId);
        }

        /// <summary>
        /// Converts the given <see cref="RnetPath"/> into a string.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected string GetPathAsString(RnetPath path)
        {
            return path.ToString();
        }

        /// <summary>
        /// Gets the URL for the given <see cref="RnetDevice"/>.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        protected Uri GetDeviceUri(RnetDevice device)
        {
            Contract.Requires<ArgumentNullException>(device != null);

            return new Uri(BaseUri, string.Format("{0}.{1}.{2}",
                (int)device.DeviceId.ControllerId,
                (int)device.DeviceId.ZoneId,
                (int)device.DeviceId.KeypadId));
        }

        /// <summary>
        /// Transforms a <see cref="RnetDevice"/> into a <see cref="Device"/>.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        protected Device RnetDeviceToInfo(RnetDevice device)
        {
            Contract.Requires<ArgumentNullException>(device != null);

            var controller = device as RnetController;
            if (controller != null)
                return new Controller()
                {
                    Href = GetDeviceUri(device),
                    Id = GetDeviceIdAsString(device.DeviceId),
                };

            return new Device()
            {
                Href = GetDeviceUri(device),
                Id = GetDeviceIdAsString(device.DeviceId),
            };
        }

    }

}
