using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

using Nancy;

using Rnet.Drivers;
using Rnet.Profiles.Core;

namespace Rnet.Service.Objects
{

    public static class Extensions
    {

        /// <summary>
        /// Makes the specified <see cref="Uri"/> relative to the request.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Uri MakeRelativeUri(this Uri uri, NancyContext context)
        {
            return new Uri(context.Request.Url.ToString().TrimEnd('/') + '/').MakeRelativeUri(uri);
        }

        /// <summary>
        /// Returns the name of the object.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static async Task<string> GetName(this RnetBusObject o, NancyContext context)
        {
            Contract.Requires<ArgumentNullException>(o != null);

            // obtain object profile
            var p = await o.GetProfile<IObject>();
            if (p == null)
                return await o.GetId();

            return p.DisplayName;
        }

        /// <summary>
        /// Gets the Uri of the object.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static async Task<Uri> GetUri(this RnetBusObject o, NancyContext context)
        {
            Contract.Requires<ArgumentNullException>(o != null);

            // obtain device URI for devices
            if (o is RnetDevice)
                return ((RnetDevice)o).GetUri(context);

            // combine with URI of owner
            var p = o.GetOwner();
            if (p != null)
                return (await p.GetUri(context)).UriCombine(await o.GetId());

            return null;
        }

        /// <summary>
        /// Gets the Uri of the profile.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static async Task<Uri> GetUri(this Profile profile, NancyContext context)
        {
            Contract.Requires<ArgumentNullException>(profile != null);

            var u = await profile.Target.GetUri(context);
            if (u == null)
                return null;

            return u.UriCombine(Util.PROFILE_URI_PREFIX + profile.Metadata.Id);
        }

        /// <summary>
        /// Converts the given <see cref="RnetDeviceId"/> into a string.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public static string GetId(this RnetDevice device)
        {
            return string.Format("{0}.{1}.{2}",
                (int)device.DeviceId.ControllerId,
                (int)device.DeviceId.ZoneId,
                (int)device.DeviceId.KeypadId);
        }

        /// <summary>
        /// Gets the URL for the given <see cref="RnetDevice"/>.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public static Uri GetUri(this RnetDevice device, NancyContext context)
        {
            Contract.Requires<ArgumentNullException>(device != null);

            return context.GetBaseUri()
                .UriCombine(":" + device.GetId());
        }

    }

}
