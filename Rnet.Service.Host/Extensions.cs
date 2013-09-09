using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using Nancy;
using Rnet.Drivers;
using Rnet.Profiles.Core;

namespace Rnet.Service.Host.Models
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
        /// Gets the Uri of the object.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static async Task<Uri> GetFriendlyUri(this RnetBusObject o, NancyContext context)
        {
            Contract.Requires<ArgumentNullException>(o != null);

            // combine with URI of container
            var p = o.GetContainer();
            if (p != null)
                return (await p.GetFriendlyUri(context)).UriCombine(await o.GetId());

            // no container, we must be under the bus directly
            return context.GetBaseUri().UriCombine(await o.GetId());
        }

        /// <summary>
        /// Gets the uri of the profile.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task<Uri> GetUri(this ProfileHandle profile, NancyContext context)
        {
            Contract.Requires<ArgumentNullException>(profile != null);
            Contract.Requires<ArgumentNullException>(context != null);

            var u = await profile.Target.GetUri(context);
            if (u == null)
                return null;

            return u.UriCombine(Util.PROFILE_URI_PREFIX + profile.Metadata.Id);
        }

        /// <summary>
        /// Gets the friendly uri of the profile.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task<Uri> GetFriendlyUri(this ProfileHandle profile, NancyContext context)
        {
            Contract.Requires<ArgumentNullException>(profile != null);
            Contract.Requires<ArgumentNullException>(context != null);

            var u = await profile.Target.GetFriendlyUri(context);
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
            Contract.Requires<ArgumentNullException>(device != null);

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

        /// <summary>
        /// Serializes the specified <see cref="Object"/> to an <see cref="XElement"/>.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static XElement ToXElement<T>(this T self)
        {
            var xml = new XDocument();
            var srs = new XmlSerializer(typeof(T));
            using (var wrt = xml.CreateWriter())
                srs.Serialize(wrt, self);

            return xml.Root;
        }

    }

}
