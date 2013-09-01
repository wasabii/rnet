using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Threading.Tasks;
using System.Xml.Linq;

using Rnet.Drivers;
using Rnet.Profiles;
using Rnet.Profiles.Metadata;

namespace Rnet.Service.Objects
{

    [ServiceContract(Namespace = "urn:rnet:service", Name = "objects")]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, UseSynchronizationContext = true)]
    [FormatServiceBehavior]
    class ObjectService : WebServiceBase
    {

        const string PROFILE_PREFIX = "+";
        const string PROFILE_METADATA_XMLNS = "urn:rnet:profiles:metadata";

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="bus"></param>
        internal ObjectService(RnetBus bus)
            : base(bus)
        {
            Contract.Requires<ArgumentNullException>(bus != null);
        }

        /// <summary>
        /// Gets the bus descriptor.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "{*uri}")]
        [ServiceKnownType(typeof(ObjectRefCollection))]
        [ServiceKnownType(typeof(Object))]
        [ServiceKnownType(typeof(ProfileRefCollection))]
        [ServiceKnownType(typeof(Profile))]
        public async Task<Message> Get(string uri)
        {
            Contract.Requires<ArgumentNullException>(uri != null);

            // if bus is down, so are we
            if (Bus.State != RnetBusState.Started ||
                Bus.Client.State != RnetClientState.Started ||
                Bus.Client.Connection.State != RnetConnectionState.Open)
                throw new WebFaultException(HttpStatusCode.ServiceUnavailable);

            // an attempt to browse the root
            if (uri == "")
                return await GetObjectRefs();

            // navigate down hierarchy until the end
            var target = await Resolve(uri.Split('/'), 0, Bus.Controllers);

            // ended up at an object
            if (target is RnetBusObject)
                return await GetObject((RnetBusObject)target);

            // ended up at a profile
            if (target is Profile)
                return await GetProfile((Profile)target);

            // ended up at profile metadata
            if (target is ProfileDescriptor)
                return await GetProfileMetadata((ProfileDescriptor)target);

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
                    Href = await GetObjectUri(i),
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
            Contract.ForAll(components, i => i != null && i.Length > 0);

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
        /// Transforms the bus object into output.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        async Task<Message> GetObject(RnetBusObject o)
        {
            Contract.Requires(o != null);

            var r = new Object()
            {
                Href = await GetObjectUri(o),
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
                    Href = await GetObjectUri(i),
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
                    Href = await GetProfileUri(i),
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
            l.Add(PROFILE_PREFIX + profile.Metadata.Id);

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
            var id = components[position++].Remove(0, PROFILE_PREFIX.Length);

            // first profile with metadata that corresponds with full profile name
            var profile = profiles.FirstOrDefault(i => i.Metadata.Id == id);
            if (profile == null)
                throw new WebFaultException(HttpStatusCode.NotFound);

            // are we at the end?
            if (position >= components.Length)
                return profile;

            // possibly can request metadata
            var next = components[position++];
            if (next == "metadata")
                return profile.Metadata;

            // as far as we can go
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

            return Context.CreateXmlResponse(await GetProfileXml(profile));
        }

        /// <summary>
        /// Transforms the <see cref="Profile"/> into an output document.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        Task<XDocument> GetProfileXml(Profile profile)
        {
            Contract.Requires<ArgumentNullException>(profile != null);

            // default namespace of profile
            var md = profile.Metadata;

            // build XML document out of properties
            var xml = new XDocument(
                new XElement(md.XmlName,
                    md.Values.Select(i =>
                        new XElement(i.XmlName,
                            i.GetValue(profile.Instance)))));

            return Task.FromResult(xml);
        }

        /// <summary>
        /// Transforms the profile metadata into output.
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        async Task<Message> GetProfileMetadata(ProfileDescriptor metadata)
        {
            Contract.Requires<ArgumentNullException>(metadata != null);

            return Context.CreateXmlResponse(await GetProfileMetadataXml(metadata));
        }

        /// <summary>
        /// Transforms the <see cref="Profile"/> into an output document.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        Task<XDocument> GetProfileMetadataXml(ProfileDescriptor metadata)
        {
            Contract.Requires<ArgumentNullException>(metadata != null);

            // default namespace of profile
            var ns = (XNamespace)PROFILE_METADATA_XMLNS;

            // build XML document out of properties
            var xml = new XDocument(
                new XElement(ns + "Profile",
                    new XElement(ns + "Id", metadata.Id),
                    new XElement(ns + "XmlNamespace", metadata.XmlName.NamespaceName),
                    new XElement(ns + "XmlName", metadata.XmlName.LocalName),
                    new XElement(ns + "Contract", metadata.Contract.FullName),
                    new XElement(ns + "Values",
                        metadata.Values.Select(i =>
                            new XElement(ns + "Value",
                                i.Name))),
                    new XElement(ns + "Operations",
                        metadata.Operations.Select(i =>
                            new XElement(ns + "Operation",
                                i.Name)))));

            return Task.FromResult(xml);
        }

    }

}
