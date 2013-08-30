using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using Rnet.Drivers;
using Rnet.Profiles;

namespace Rnet.Service.Objects
{

    [ServiceContract(Namespace = "urn:rnet:service", Name = "objects")]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, UseSynchronizationContext = true)]
    [FormatServiceBehavior]
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
        public async Task<Message> Get(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
                return await GetObjectRefs();

            // navigate down hierarchy until the end
            var target = await Resolve(uri.Split('/'), 0, Bus.Controllers);

            // ended up at an object
            if (target is RnetBusObject)
                return await GetObject((RnetBusObject)target);

            // ended up at a profile
            if (target is Profile)
                return await GetProfile((Profile)target);

            // no idea what we ended up at
            throw new WebFaultException(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Gets references for every root object.
        /// </summary>
        /// <returns></returns>
        async Task<Message> GetObjectRefs()
        {
            var c = new ObjectRefCollection();

            // assembly references
            foreach (var i in Bus.Controllers)
                c.Add(new ObjectRef()
                {
                    Id = await GetObjectUri(i),
                    Name = await GetObjectName(i),
                });

            return Context.CreateXmlResponse<ObjectRefCollection>(c);
        }

        /// <summary>
        /// Begins navigating down the URL components starting from a set of objects.
        /// </summary>
        /// <param name="components"></param>
        /// <param name="position"></param>
        /// <param name="objects"></param>
        /// <returns></returns>
        async Task<object> Resolve(string[] components, int position, IEnumerable<RnetBusObject> objects)
        {
            Contract.Requires<ArgumentNullException>(components != null);
            Contract.Requires<ArgumentOutOfRangeException>(position >= 0);
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
                    return await ResolveProfile(components, i, o);
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
        async Task<Message> GetObject(RnetBusObject o)
        {
            Contract.Requires<ArgumentNullException>(o != null);

            var r = new Object()
            {
                Id = await GetObjectUri(o),
                Name = await GetObjectName(o),
                Objects = await GetObjectRefs(o),
                Profiles = await GetProfileRefs(o),
            };

            return Context.CreateXmlResponse<Object>(r);
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
                    Id = await GetProfileUri(i),
                    Name = await GetProfileName(i),
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
            l.Add(PROFILE_PREFIX + await GetProfileName(profile));

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
        /// Returns the name of the profile.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        Task<string> GetProfileName(Profile profile)
        {
            Contract.Requires<ArgumentNullException>(profile != null);

            return Task.FromResult<string>(profile.Metadata.Namespace + ":" + profile.Metadata.Name);
        }

        /// <summary>
        /// Navigate at a profile path.
        /// </summary>
        /// <param name="components"></param>
        /// <param name="position"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        async Task<object> ResolveProfile(string[] components, int position, RnetBusObject target)
        {
            Contract.Requires<ArgumentNullException>(components != null);
            Contract.Requires<ArgumentOutOfRangeException>(position >= 1);
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

            // are we at the end?
            if (position >= components.Length)
                return profile;

            // cannot go deeper than a profile
            throw new WebFaultException(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Transforms the profile into output.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        async Task<Message> GetProfile(Profile profile)
        {
            Contract.Requires<ArgumentNullException>(profile != null);

            return Context.CreateXmlResponse(ProfileToXml(profile));
        }

        /// <summary>
        /// Transforms the <see cref="Profile"/> into an output document.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        async Task<XDocument> ProfileToXml(Profile profile)
        {
            // default namespace of profile
            var ns = (XNamespace)profile.Metadata.Namespace;

            // build XML document out of properties
            var xml = new XDocument(
                new XElement(ns + profile.Metadata.Name,
                    profile.Metadata.Properties.Cast<PropertyDescriptor>()
                        .Select(i =>
                            new XElement(ns + i.Name,
                                i.GetValue(profile.Instance)))));
        }

    }

}
