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
        public static async Task<string> GetObjectName(this RnetBusObject o, NancyContext context)
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
        public static async Task<Uri> GetObjectUri(this RnetBusObject o, NancyContext context)
        {
            Contract.Requires<ArgumentNullException>(o != null);

            var l = new List<string>();

            do
            {
                // obtain ID of current item
                var i = await o.GetId();
                Contract.Assert(i != null);

                // add to list and recurse
                l.Add(i);
                o = o.GetContainer();
            }
            while (o != null);

            // assemble Url from components backwards
            var uri = context.GetBaseUri();
            foreach (var i in l.Reverse<string>())
                uri = uri.UriCombine(i);

            return uri;
        }

        /// <summary>
        /// Gets the Uri of the profile.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static async Task<Uri> GetProfileUri(this Profile profile, NancyContext context)
        {
            Contract.Requires<ArgumentNullException>(profile != null);

            // url ends with profile path
            var l = new List<string>();
            l.Add(Util.PROFILE_URI_PREFIX + profile.Metadata.Id);

            // begin from target object
            var target = profile.Target;

            do
            {
                // obtain ID of current item
                var id = await target.GetId();
                Contract.Assert(id != null);

                // add to list and recurse
                l.Add(id);
                target = target.GetContainer();
            }
            while (target != null);

            // assemble Url from components backwards
            var uri = context.GetBaseUri();
            foreach (var i in l.Reverse<string>())
                uri = uri.UriCombine(i);

            return uri;
        }

        /// <summary>
        /// Converts the given <see cref="RnetDeviceId"/> into a string.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public static string GetDeviceIdAsString(this RnetDevice device)
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
        public static Uri GetDeviceUri(this RnetDevice device, NancyContext context)
        {
            Contract.Requires<ArgumentNullException>(device != null);

            return context.GetBaseUri()
                .UriCombine(":" + device.GetDeviceIdAsString());
        }

    }

}
