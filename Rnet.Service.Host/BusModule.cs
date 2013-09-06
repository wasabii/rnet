using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

using Nancy;

using Rnet.Drivers;
using Rnet.Profiles.Core;
using Rnet.Service.Host.Models;
using Rnet.Service.Host.Processors;

namespace Rnet.Service.Host
{

    /// <summary>
    /// Serves requests under the objects URL.
    /// </summary>
    [Export(typeof(INancyModule))]
    [Export(typeof(BusModule))]
    public sealed class BusModule : NancyModule
    {

        ICompositionService composition;
        RnetBus bus;
        IEnumerable<Lazy<IRequestProcessor, RequestProcessorMetadata>> processors;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="bus"></param>
        [ImportingConstructor]
        public BusModule(
            [Import] ICompositionService composition,
            [Import] RnetBus bus,
            [ImportMany] IEnumerable<Lazy<IRequestProcessor, RequestProcessorMetadata>> processors)
            : base("/")
        {
            Contract.Requires<ArgumentNullException>(composition != null);
            Contract.Requires<ArgumentNullException>(bus != null);
            Contract.Requires<ArgumentNullException>(processors != null);

            this.composition = composition;
            this.bus = bus;
            this.processors = processors;

            Get[@"/", true] =
            Get[@"/{Uri*}", true] = async (x, ct) =>
                await GetRequest(x.Uri);

            Put[@"/", true] =
            Put[@"/{Uri*}", true] = async (x, ct) =>
                await PutRequest(x.Uri);
        }

        /// <summary>
        /// Implements a GET request.
        /// </summary>
        /// <returns></returns>
        async Task<object> GetRequest(string uri)
        {
            // split and clean up uri
            var path = uri != null ? uri.Split('/')
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .ToArray() : new string[0];

            // resolve the path into an object
            var o = await Resolve(bus, path) ?? HttpStatusCode.NotFound;
            if (o is HttpStatusCode ||
                o is Response)
                return o;

            // handle GET request
            return await InvokeGet(o) ?? HttpStatusCode.MethodNotAllowed;
        }

        /// <summary>
        /// Implements a PUT request.
        /// </summary>
        /// <returns></returns>
        async Task<object> PutRequest(string uri)
        {
            Contract.Requires<ArgumentNullException>(uri != null);

            // split and clean up uri
            var path = uri.Split('/')
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .ToArray();

            // resolve the path into an object
            var o = await Resolve(bus, path);
            if (o == null)
                return HttpStatusCode.NotFound;

            // handle GET request
            return await InvokePut(o) ?? HttpStatusCode.NoContent;
        }

        /// <summary>
        /// Fully resolves the path from the given object.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        async Task<object> Resolve(object source, string[] path)
        {
            var o = await InvokeResolve(source, path);
            while (o is ResolveResponse)
                o = await InvokeResolve(((ResolveResponse)o).Object, ((ResolveResponse)o).Path);

            return o;
        }

        /// <summary>
        /// Invokes the matching <see cref="IRequestProcessor"/> for the given source object, using the provided function.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="invoke"></param>
        /// <returns></returns>
        async Task<object> Invoke(object source, Func<IRequestProcessor, Task<object>> invoke)
        {
            var o = source;

            foreach (var p in processors.OrderByDescending(i => i.Metadata.Infos.Max(j => j.Priority)))
                if (p.Metadata.Infos.Any(i => i.Type.IsInstanceOfType(source)))
                    if ((o = await invoke(p.Value)) != null)
                        break;

            return o;
        }

        /// <summary>
        /// Invokes the request processors in order to resolve one level.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<object> InvokeResolve(object source, string[] path)
        {
            return Invoke(source, i => i.Resolve(source, path));
        }

        /// <summary>
        /// Invokes the request processors in order to handle a GET request.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<object> InvokeGet(object source)
        {
            return Invoke(source, i => i.Get(source));
        }

        /// <summary>
        /// Invokes the request processors in order to handle a PUT request.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<object> InvokePut(object source)
        {
            return Invoke(source, i => i.Put(source));
        }

        /// <summary>
        /// Searches the given set of <see cref="RnetBusObject"/>s for the one with the matching ID.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        internal async Task<RnetBusObject> FindObject(IEnumerable<RnetBusObject> source, string id)
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
        /// Transforms the given <see cref="RnetBusObject"/> into a <see cref="ObjectData"/> instance.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public async Task<ObjectData> ObjectToData(RnetBusObject d)
        {
            if (d is RnetDevice)
                return await DeviceToData((RnetDevice)d);
            else
                return await FillObjectData(d, new ObjectData());
        }

        async Task<ObjectData> FillObjectData(RnetBusObject o, ObjectData d)
        {
            d.Uri = await o.GetUri(Context);
            d.FriendlyUri = await o.GetFriendlyUri(Context);
            d.Id = await o.GetId();
            d.Name = await o.GetName(Context);
            d.Objects = await GetObjects(o);
            d.Profiles = await GetProfileRefs(o);
            return d;
        }

        /// <summary>
        /// Transforms the given <see cref="RnetDevice"/> into a <see cref="DeviceData"/> instance.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public async Task<DeviceData> DeviceToData(RnetDevice d)
        {
            if (d is RnetController)
                return await ControllerToData((RnetController)d);
            else
                return await FillDeviceData(d, new DeviceData());
        }

        async Task<DeviceData> FillDeviceData(RnetDevice o, DeviceData d)
        {
            await FillObjectData(o, d);
            d.RnetId = o.GetId();
            d.DataUri = o.GetUri(Context).UriCombine(Util.DATA_URI_SEGMENT);
            return d;
        }

        /// <summary>
        /// Transforms the given <see cref="RnetController"/> into a <see cref="ControllerData"/> instance.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public async Task<ControllerData> ControllerToData(RnetController d)
        {
            return await FillControllerData(d, new ControllerData());
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
            return new ObjectDataCollection(await Task.WhenAll(p.Select(i => ObjectToData(i))));
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
            var p = await o.GetProfiles() ?? Enumerable.Empty<ProfileHandle>();
            return new ProfileRefCollection(await Task.WhenAll(p.Select(i => ProfileToRef(i))));
        }

        public async Task<ProfileRef> ProfileToRef(ProfileHandle profile)
        {
            return new ProfileRef()
            {
                Uri = await profile.GetUri(Context),
                FriendlyUri = await profile.GetFriendlyUri(Context),
                Id = profile.Metadata.Id,
            };
        }

    }

}
