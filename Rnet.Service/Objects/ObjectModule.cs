using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

using Nancy;

using Rnet.Drivers;
using Rnet.Profiles.Core;
using Rnet.Profiles.Metadata;

namespace Rnet.Service.Objects
{

    [Export(typeof(INancyModule))]
    public sealed class ObjectModule : NancyModuleBase
    {

        const string PROFILE_URI_PREFIX = "~";

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="bus"></param>
        [ImportingConstructor]
        public ObjectModule(RnetBus bus)
            : base(bus, "/objects")
        {
            Contract.Requires<ArgumentNullException>(bus != null);

            Get["/", true] = async (x, ct) => await GetUri("");
            Get["/{Uri*}", true] = async (x, ct) => await GetUri(x.Uri);
        }

        /// <summary>
        /// Gets the bus descriptor.
        /// </summary>
        /// <returns></returns>
        async Task<dynamic> GetUri(string uri)
        {
            Contract.Requires<ArgumentNullException>(uri != null);

            // if bus is down, so are we
            if (Bus.State != RnetBusState.Started ||
                Bus.Client.State != RnetClientState.Started ||
                Bus.Client.Connection.State != RnetConnectionState.Open)
                return HttpStatusCode.ServiceUnavailable;

            // an attempt to browse the root
            if (uri == "")
                return await GetObjectRefs();

            // navigate down hierarchy until the end
            var target = await Resolve(uri.Split('/'), 0, Bus.Controllers);
            if (target == null)
                return HttpStatusCode.BadRequest;

            return target;
        }

        /// <summary>
        /// Gets references for every root object.
        /// </summary>
        /// <returns></returns>
        async Task<ObjectRefCollection> GetObjectRefs()
        {
            var c = new ObjectRefCollection();

            // assembly references
            foreach (var i in Bus.Controllers)
                c.Add(new ObjectRef()
                {
                    Href = MakeRelativeUri(await GetObjectUri(i)),
                    Name = await GetObjectName(i),
                });

            return c;
        }

        /// <summary>
        /// Begins navigating down the URL components starting from a set of objects.
        /// </summary>
        /// <param name="components"></param>
        /// <param name="position"></param>
        /// <param name="objects"></param>
        /// <returns></returns>
        async Task<dynamic> Resolve(string[] components, int position, IEnumerable<RnetBusObject> objects)
        {
            Contract.Requires<ArgumentNullException>(components != null);
            Contract.Requires<ArgumentOutOfRangeException>(position >= 0);
            Contract.Requires<ArgumentNullException>(objects != null);
            Contract.ForAll(components, i => i != null && i.Length > 0);

            var o = (RnetBusObject)null;

            // repeat for each path item
            for (int i = position; i < components.Length; i++)
            {
                // component refers to a profile
                if (components[i].StartsWith(PROFILE_URI_PREFIX))
                {
                    // cannot do this if no current object known
                    if (o == null)
                        return HttpStatusCode.BadRequest;

                    // begin navigating from current object
                    return await ResolveProfile(components, i, o);
                }

                // find object with matching ID
                o = await FindObject(objects, components[i]);
                if (o == null)
                    return HttpStatusCode.NotFound;

                // next container to work on is this one
                objects = await o.GetProfile<IContainer>();
            }

            // item not found; at end of path
            if (o == null)
                return HttpStatusCode.NotFound;

            // return the object we ended at, wrapped
            return new ObjectData()
            {
                Id = await o.GetId(),
                Name = await GetObjectName(o),
                Objects = await GetObjectRefs(o),
                Profiles = await GetProfileRefs(o),
            };
        }

        /// <summary>
        /// Searches the given set of <see cref="RnetBusObject"/>s for the one with the matching ID.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        async Task<RnetBusObject> FindObject(IEnumerable<RnetBusObject> source, string id)
        {
            Contract.Requires(source != null);
            Contract.Requires(id != null);

            // find first matching ID
            foreach (var o in source)
            {
                var i = await o.GetId();
                if (i == id)
                    return o;
            }

            return null;
        }

        /// <summary>
        /// Gets the child objects refs for the given object.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        async Task<ObjectRefCollection> GetObjectRefs(RnetBusObject o)
        {
            Contract.Requires<ArgumentNullException>(o != null);

            // load container
            var p = await o.GetProfile<IContainer>() ?? Enumerable.Empty<RnetBusObject>();
            var c = new ObjectRefCollection();

            // assembly references
            foreach (var i in p)
                c.Add(new ObjectRef()
                {
                    Href = MakeRelativeUri(await GetObjectUri(i)),
                    Name = await GetObjectName(i),
                });

            return c;
        }

        /// <summary>
        /// Gets the Uri of the object.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        async Task<Uri> GetObjectUri(RnetBusObject o)
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
            var uri = BaseUri;
            foreach (var i in l.Reverse<string>())
                uri = uri.UriCombine(i);

            return uri;
        }

        /// <summary>
        /// Returns the name of the object.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        async Task<string> GetObjectName(RnetBusObject o)
        {
            Contract.Requires<ArgumentNullException>(o != null);

            // obtain object profile
            var p = await o.GetProfile<IObject>();
            if (p == null)
                return await o.GetId();

            return p.DisplayName;
        }

        /// <summary>
        /// Gets the profile refs for the given object.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        async Task<ProfileRefCollection> GetProfileRefs(RnetBusObject o)
        {
            Contract.Requires<ArgumentNullException>(o != null);

            // load container
            var p = await o.GetProfiles() ?? Enumerable.Empty<Profile>();
            var c = new ProfileRefCollection();

            // assembly references
            foreach (var i in p)
                c.Add(new ProfileRef()
                {
                    Href = MakeRelativeUri(await GetProfileUri(i)),
                    Id = i.Metadata.Id,
                });

            return c;
        }

        /// <summary>
        /// Gets the Uri of the profile.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        async Task<Uri> GetProfileUri(Profile profile)
        {
            Contract.Requires<ArgumentNullException>(profile != null);

            // url ends with profile path
            var l = new List<string>();
            l.Add(PROFILE_URI_PREFIX + profile.Metadata.Id);

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
            var uri = BaseUri;
            foreach (var i in l.Reverse<string>())
                uri = uri.UriCombine(i);

            return uri;
        }

        /// <summary>
        /// Navigate at a profile path.
        /// </summary>
        /// <param name="components"></param>
        /// <param name="position"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        async Task<dynamic> ResolveProfile(string[] components, int position, RnetBusObject target)
        {
            Contract.Requires<ArgumentNullException>(components != null);
            Contract.Requires<ArgumentOutOfRangeException>(position >= 1);
            Contract.Requires<ArgumentNullException>(target != null);

            // find matching profile
            var profiles = await target.GetProfiles();
            if (profiles == null)
                return HttpStatusCode.NotFound;

            // full profile name
            var id = components[position++].Remove(0, PROFILE_URI_PREFIX.Length);

            // first profile with metadata that corresponds with full profile name
            var profile = profiles.FirstOrDefault(i => i.Metadata.Id == id);
            if (profile == null)
                return HttpStatusCode.NotFound;

            // are we at the end?
            if (position >= components.Length)
                return profile;

            // possibly can request metadata
            var next = components[position++];
            if (next == "metadata")
                return profile.Metadata;

            // as far as we can go
            return HttpStatusCode.NotFound;
        }

        /// <summary>
        /// Makes the specified <see cref="Uri"/> relative to the request.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        Uri MakeRelativeUri(Uri uri)
        {
            return new Uri(Request.Url.ToString().TrimEnd('/') + '/').MakeRelativeUri(uri);
        }

    }

}
