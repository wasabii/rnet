using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading.Tasks;

using Rnet.Drivers;
using Rnet.Profiles;

namespace Rnet.Service.Objects
{

    [ServiceContract]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, UseSynchronizationContext = true)]
    class ObjectService : RnetWebServiceBase
    {

        const string PROFILE_PREFIX = "~";

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="bus"></param>
        public ObjectService(RnetBus bus)
            : base(bus)
        {

        }

        /// <summary>
        /// Gets the bus descriptor.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "{*uri}")]
        [ServiceKnownType(typeof(ObjectRefCollection))]
        [ServiceKnownType(typeof(Object))]
        public async Task<object> Get(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
                throw new WebFaultException(HttpStatusCode.BadRequest);

            // navigate down hierarchy until the end
            var target = await NavigateAtObject(uri.Split('/'), 0, Bus.Controllers);

            // ended up at an object
            if (target is RnetBusObject)
                return GetObject((RnetBusObject)target);

            // ended up at a profile
            if (target is Profile)
                return GetProfile((Profile)target);

            // no idea what we ended up at
            throw new WebFaultException(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Begins navigating down the URL components starting from a set of objects.
        /// </summary>
        /// <param name="components"></param>
        /// <param name="position"></param>
        /// <param name="objects"></param>
        /// <returns></returns>
        async Task<object> NavigateAtObject(string[] components, int position, IEnumerable<RnetBusObject> objects)
        {
            Contract.Requires<ArgumentNullException>(components != null);
            Contract.Requires<ArgumentOutOfRangeException>(position < 0);
            Contract.Requires<ArgumentNullException>(objects != null);

            var o = (RnetBusObject)null;

            // repeat for each path item
            for (int i = position; i < components.Length; i++)
            {
                // component refers to a profile
                if (components[i].StartsWith(PROFILE_PREFIX))
                {
                    // cannot do this if no current object known
                    if (o == null)
                        throw new WebFaultException(HttpStatusCode.BadRequest);

                    // begin navigating from current object
                    return await NavigateAtProfile(components, i, o);
                }

                // find object with matching ID
                o = await FindObject(objects, components[i]);
                if (o == null)
                    throw new WebFaultException(HttpStatusCode.NotFound);

                // next container to work on is this one
                objects = await o.GetProfile<IContainer>();
            }

            // item not found; at end of path
            if (o == null)
                throw new WebFaultException(HttpStatusCode.NotFound);

            // return the object we ended at
            return o;
        }

        /// <summary>
        /// Searches the given set of <see cref="RnetBusObject"/>s for the one with the matching ID.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        async Task<RnetBusObject> FindObject(IEnumerable<RnetBusObject> source, string id)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(id != null);

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
        /// Transforms the bus object into output.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        async Task<Object> GetObject(RnetBusObject o)
        {
            Contract.Requires<ArgumentNullException>(o != null);

            return new Object()
            {
                Id = await GetObjectUri(o),
                Name = await GetObjectName(o),
                Objects = await GetObjectRefs(o),
            };
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
                    Id = await GetObjectUri(i),
                    Name = await GetObjectName(i),
                });

            return c;
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
                uri = new Uri(uri, i);

            return uri;
        }

        /// <summary>
        /// Navigate at a profile path.
        /// </summary>
        /// <param name="components"></param>
        /// <param name="position"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        async Task<object> NavigateAtProfile(string[] components, int position, RnetBusObject target)
        {
            Contract.Requires<ArgumentNullException>(components != null);
            Contract.Requires<ArgumentOutOfRangeException>(position < 0);
            Contract.Requires<ArgumentNullException>(target != null);

            // find matching profile
            var profiles = await target.GetProfiles();
            if (profiles == null)
                throw new WebFaultException(HttpStatusCode.NotFound);

            // full profile name
            var name = components[position++].Remove(0, PROFILE_PREFIX.Length);

            // first profile with metadata that corresponds with full profile name
            var profile = profiles.FirstOrDefault(i => i.Metadata.Namespace + ":" + i.Metadata.Name == name);
            if (profile == null)
                throw new WebFaultException(HttpStatusCode.NotFound);

            // navigate into the profile itself
            return await NavigateIntoProfile(components, position, profile);
        }

        /// <summary>
        /// Navigate into 
        /// </summary>
        /// <param name="components"></param>
        /// <param name="position"></param>
        /// <param name="profile"></param>
        /// <returns></returns>
        async Task<object> NavigateIntoProfile(string[] components, int position, Profile profile)
        {
            Contract.Requires<ArgumentNullException>(components != null);
            Contract.Requires<ArgumentOutOfRangeException>(position < 1);
            Contract.Requires<ArgumentNullException>(profile != null);

            // we terminated in a profile
            if (position >= components.Length)
                return profile;

            throw new NotImplementedException("Navigation within a Profile is not yet supported.");
        }

        /// <summary>
        /// Transforms the profile into output.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        async Task<object> GetProfile(Profile profile)
        {
            Contract.Requires<ArgumentNullException>(profile != null);

            // temporary
            return profile.Metadata.Namespace + ":" + profile.Metadata.Name;
        }

    }

}
