using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Nancy;
using Nancy.Responses;
using Rnet.Drivers;
using Rnet.Profiles.Core;
using Rnet.Profiles.Metadata;
using Rnet.Service.Processors;

namespace Rnet.Service.Objects
{

    /// <summary>
    /// Serves requests under the objects URL.
    /// </summary>
    [Export(typeof(INancyModule))]
    [Export(typeof(ObjectModule))]
    public sealed class ObjectModule : BusModule
    {

        ICompositionService composition;
        IEnumerable<ExportFactory<IRequestProcessor, IRequestProcessorMetadata>> processors;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="bus"></param>
        [ImportingConstructor]
        public ObjectModule(
            [Import] ICompositionService composition,
            [Import] RnetBus bus,
            [ImportMany] IEnumerable<ExportFactory<IRequestProcessor, IRequestProcessorMetadata>> processors)
            : base(bus, "/")
        {
            Contract.Requires<ArgumentNullException>(composition != null);
            Contract.Requires<ArgumentNullException>(bus != null);
            Contract.Requires<ArgumentNullException>(processors != null);

            this.composition = composition;
            this.processors = processors;

            Get[@"/", c => !c.Request.Url.Path.StartsWith("/:"), true] = async (x, ct) => await GetRequest("");
            Get[@"/{Uri*}", c => !c.Request.Url.Path.StartsWith("/:"), true] = async (x, ct) => await GetRequest(x.Uri);
        }

        /// <summary>
        /// Discovered target.
        /// </summary>
        [Export]
        public RequestTarget Target { get; private set; }

        /// <summary>
        /// Gets the device description.
        /// </summary>
        /// <param name="controllerId"></param>
        /// <param name="zoneId"></param>
        /// <param name="keypadId"></param>
        /// <returns></returns>
        Task<DeviceData> GetDevice(string controllerId, string zoneId, string keypadId)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(controllerId));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(zoneId));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(keypadId));

            return DeviceToData(GetRnetDevice(controllerId, zoneId, keypadId));
        }

        /// <summary>
        /// Gets the device data.
        /// </summary>
        /// <param name="controllerId"></param>
        /// <param name="zoneId"></param>
        /// <param name="keypadId"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        async Task<Response> GetDeviceData(string controllerId, string zoneId, string keypadId, string path)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(controllerId));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(zoneId));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(keypadId));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path));

            return await GetDeviceData(GetRnetDevice(controllerId, zoneId, keypadId), path);
        }

        /// <summary>
        /// Gets the <see cref="IRnetZoneDevice"/> given by the IDs.
        /// </summary>
        /// <param name="controllerId"></param>
        /// <param name="zoneId"></param>
        /// <param name="keypadId"></param>
        /// <returns></returns>
        RnetDevice GetRnetDevice(string controllerId, string zoneId, string keypadId)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(controllerId));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(zoneId));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(keypadId));

            return Bus[byte.Parse(controllerId), byte.Parse(zoneId), byte.Parse(keypadId)];
        }

        /// <summary>
        /// Gets the data from the given path of the device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        async Task<dynamic> GetDeviceData(RnetDevice device, string path)
        {
            Contract.Requires<ArgumentNullException>(device != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path));

            var handle = device[RnetPath.Parse(path.Replace('/', '.'))];
            if (handle == null)
                return HttpStatusCode.NotFound;

            //// check for cache lifetime
            //IncomingRequest.CheckConditionalRetrieve(handle.Timestamp);

            //// refresh if requested
            //CacheControlHeaderValue cc;
            //if (CacheControlHeaderValue.TryParse(IncomingRequest.Headers[HttpRequestHeader.CacheControl], out cc))
            //    if (cc.MustRevalidate)
            //        await handle.Refresh();

            // read data
            var data = await handle.Read();
            if (data == null)
                return HttpStatusCode.NotFound;

            return Response.FromStream(new MemoryStream(data), "application/octet-stream")
                .WithHeader("Last-Modified", handle.Timestamp.ToString("R"));
        }

        /// <summary>
        /// Implements a GET request.
        /// </summary>
        /// <returns></returns>
        async Task<object> GetRequest(string path)
        {
            Contract.Requires<ArgumentNullException>(path != null);

            // split and clean up uri
            var uri = path.Split('/')
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .ToArray();

            // resolve targeted object
            var o = await Resolve(Bus, uri, 0);
            if (o == null)
                return HttpStatusCode.NotFound;

            // store target
            Target = new RequestTarget(uri, o);

            // process request
            return await processors
                .Where(i => i.Metadata.Type == o.GetType())
                .ToObservable()
                .SelectAsync(i => i.CreateExport().Value.Get(), true)
                .FirstOrDefaultAsync(i => i != null);
        }

        /// <summary>
        /// Resolves the given Uri segment from the point of view of the specified object.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="uri"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        async Task<object> Resolve(object o, string[] uri, int position)
        {
            if (o is RnetBus)
                return await Resolve((RnetBus)o, uri, position);
            if (o is RnetBusObject)
                return await Resolve((RnetBusObject)o, uri, position);
            if (o is Profile)
                return await Resolve((Profile)o, uri, position);

            return null;
        }

        /// <summary>
        /// Resolves the given url segment from the bus.
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="uri"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        async Task<object> Resolve(RnetBus bus, string[] uri, int position)
        {
            // if bus is down, so are we
            if (Bus.State != RnetBusState.Started ||
                Bus.Client.State != RnetClientState.Started ||
                Bus.Client.Connection.State != RnetConnectionState.Open)
                return HttpStatusCode.ServiceUnavailable;

            // end of uri, request is for bus itself
            if (position >= uri.Length)
                return bus;

            // path represents a direct device ID
            if (uri[position][0] == ':')
                return GetRnetDevice(uri[position].Substring(1));

            // resolve controller
            var o = await FindObject(bus.Controllers, uri[position]);
            if (o != null)
                return await Resolve(o, uri, position + 1);

            return null;
        }

        /// <summary>
        /// Resolves the given url segment from the given bus object.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="uri"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        async Task<object> Resolve(RnetBusObject target, string[] uri, int position)
        {
            // end of uri, request is for object itself
            if (position >= uri.Length)
                return target;

            // referring to a profile
            if (uri[position].StartsWith(Util.PROFILE_URI_PREFIX))
                return await ResolveProfile(target, uri, position, uri[position].Substring(Util.PROFILE_URI_PREFIX.Length));

            // object contains other objects
            var c = await target.GetProfile<IContainer>();
            if (c != null)
            {
                // find contained object with specified id
                var o = await FindObject(c, uri[position]);
                if (o != null)
                    return await Resolve(o, uri, position + 1);
            }

            return null;
        }

        /// <summary>
        /// Navigate at a profile path.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="position"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        async Task<object> ResolveProfile(RnetBusObject target, string[] uri, int position, string id)
        {
            // find matching profile
            var profiles = await target.GetProfiles();
            if (profiles == null)
                return null;

            // first profile with metadata that corresponds with uri
            var profile = profiles.FirstOrDefault(i => i.Metadata.Id == id);
            if (profile != null)
                return await Resolve(profile, uri, position + 1);

            return null;
        }

        /// <summary>
        /// Resolves the given url segment from the given profile.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="uri"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        async Task<object> Resolve(Profile profile, string[] uri, int position)
        {
            // end of uri, request is for object itself
            if (position >= uri.Length)
                return profile;

            // resolve property
            var property = profile.Metadata.Properties
                .FirstOrDefault(i => i.Name == uri[position]);
            if (property != null)
                return await Resolve(profile, property, uri, position);

            // resolve command
            var command = profile.Metadata.Operations
                .FirstOrDefault(i => i.Name == uri[position]);
            if (command != null)
                return await Resolve(profile, command, uri, position);

            return null;
        }

        /// <summary>
        /// Resolves the given url segment from the given property.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="property"></param>
        /// <param name="uri"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        Task<object> Resolve(Profile profile, PropertyDescriptor property, string[] uri, int position)
        {
            if (position >= uri.Length)
                return Task.FromResult<object>(property);

            return null;
        }

        /// <summary>
        /// Resolves the given url segment from the given command.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="command"></param>
        /// <param name="uri"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        Task<object> Resolve(Profile profile, CommandDescriptor command, string[] uri, int position)
        {
            if (position >= uri.Length)
                return Task.FromResult<object>(command);

            return null;
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

        public async Task<ObjectData> ObjectToData(RnetBusObject d)
        {
            if (d is RnetDevice)
                return await DeviceToData((RnetDevice)d);
            else
                return await FillObjectData(d, new ObjectData());
        }

        public async Task<DeviceData> DeviceToData(RnetDevice d)
        {
            if (d is RnetController)
                return await ControllerToData((RnetController)d);
            else
                return await FillDeviceData(d, new DeviceData());
        }

        public async Task<ControllerData> ControllerToData(RnetController d)
        {
            return await FillControllerData(d, new ControllerData());
        }

        async Task<ObjectData> FillObjectData(RnetBusObject o, ObjectData d)
        {
            d.Id = await o.GetId();
            d.Href = await o.GetObjectUri(Context);
            d.Name = await o.GetObjectName(Context);
            d.Objects = await GetObjects(o);
            d.Profiles = await GetProfileRefs(o);
            return d;
        }

        async Task<DeviceData> FillDeviceData(RnetDevice o, DeviceData d)
        {
            await FillObjectData(o, d);
            d.DeviceHref = o.GetDeviceUri(Context);
            d.DeviceId = o.GetDeviceIdAsString();
            return d;
        }

        async Task<ControllerData> FillControllerData(RnetController o, ControllerData d)
        {
            await FillDeviceData(o, d);
            return d;
        }

        /// <summary>
        /// Gets the child objects refs for the given object.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public async Task<ObjectDataCollection> GetObjects(RnetBusObject o)
        {
            Contract.Requires<ArgumentNullException>(o != null);

            // load container
            var p = await o.GetProfile<IContainer>() ?? Enumerable.Empty<RnetBusObject>();
            return new ObjectDataCollection(await p.ToObservable().SelectAsync(i => ObjectToData(i), true).ToList());
        }

        /// <summary>
        /// Gets the profile refs for the given object.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public async Task<ProfileRefCollection> GetProfileRefs(RnetBusObject o)
        {
            Contract.Requires<ArgumentNullException>(o != null);

            // load container
            var p = await o.GetProfiles() ?? Enumerable.Empty<Profile>();
            return new ProfileRefCollection(await p.ToObservable().SelectAsync(i => ProfileToRef(i), true).ToList());
        }

        public async Task<ProfileRef> ProfileToRef(Profile profile)
        {
            return new ProfileRef()
            {
                Href = (await profile.GetProfileUri(Context)).MakeRelativeUri(Context),
                Id = profile.Metadata.Id,
            };
        }

    }

}
