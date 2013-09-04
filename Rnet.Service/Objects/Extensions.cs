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
        /// Gets the child objects refs for the given object.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static async Task<ObjectRefCollection> GetObjectRefs(this RnetBusObject o, NancyContext context)
        {
            Contract.Requires<ArgumentNullException>(o != null);

            // load container
            var p = await o.GetProfile<IContainer>() ?? Enumerable.Empty<RnetBusObject>();
            var c = new ObjectRefCollection();

            // assembly references
            foreach (var i in p)
                c.Add(new ObjectRef()
                {
                    Href = (await i.GetObjectUri(context)).MakeRelativeUri(context),
                    Name = await i.GetObjectName(context),
                });

            return c;
        }

        /// <summary>
        /// Gets the profile refs for the given object.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static async Task<ProfileRefCollection> GetProfileRefs(this RnetBusObject o, NancyContext context)
        {
            Contract.Requires<ArgumentNullException>(o != null);

            // load container
            var p = await o.GetProfiles() ?? Enumerable.Empty<Profile>();
            var c = new ProfileRefCollection();

            // assembly references
            foreach (var i in p)
                c.Add(new ProfileRef()
                {
                    Href = (await i.GetProfileUri(context)).MakeRelativeUri(context),
                    Id = i.Metadata.Id,
                });

            return c;
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

    }

}
