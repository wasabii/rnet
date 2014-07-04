using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Rnet.Drivers;
using Rnet.Profiles.Core;
using Rnet.Service.Host.Models;
using Rnet.Service.Host.Processors;

namespace Rnet.Service.Host
{

    /// <summary>
    /// Serves requests under the objects URL.
    /// </summary>
    [Export(typeof(RootProcessor))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class RootProcessor
    {

        readonly ICompositionService composition;
        readonly RnetBus bus;
        readonly DriverManager driverManager;
        readonly ProfileManager profileManager;
        readonly IEnumerable<Lazy<IRequestProcessor, RequestProcessorMetadata>> requestProcessors;
        readonly IEnumerable<Lazy<IResponseProcessor, ResponseProcessorMetadata>> responseProcessors;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="composition"></param>
        /// <param name="bus"></param>
        /// <param name="driverManager"></param>
        /// <param name="profileManager"></param>
        /// <param name="requestProcessors"></param>
        /// <param name="responseProcessors"></param>
        [ImportingConstructor]
        public RootProcessor(
            [Import] ICompositionService composition,
            [Import] RnetBus bus,
            [Import] DriverManager driverManager,
            [Import] ProfileManager profileManager,
            [ImportMany] IEnumerable<Lazy<IRequestProcessor, RequestProcessorMetadata>> requestProcessors,
            [ImportMany] IEnumerable<Lazy<IResponseProcessor, ResponseProcessorMetadata>> responseProcessors)
        {
            Contract.Requires<ArgumentNullException>(composition != null);
            Contract.Requires<ArgumentNullException>(bus != null);
            Contract.Requires<ArgumentNullException>(requestProcessors != null);
            Contract.Requires<ArgumentNullException>(responseProcessors != null);
            Contract.Requires<ArgumentNullException>(driverManager != null);
            Contract.Requires<ArgumentNullException>(profileManager != null);

            this.composition = composition;
            this.bus = bus;
            this.driverManager = driverManager;
            this.profileManager = profileManager;
            this.requestProcessors = requestProcessors;
            this.responseProcessors = responseProcessors;
        }

        /// <summary>
        /// Handles an incoming request.
        /// </summary>
        /// <param name="context"></param>
        public async Task Invoke(IContext context)
        {
            // handle index file
            if (context.Request.Path.Value == "/index.html")
            {
                using (var mst = new MemoryStream())
                using (var stm = typeof(RootProcessor).Assembly.GetManifestResourceStream("Rnet.Service.Host.index.html"))
                {
                    context.Response.ContentType = "text/html";
                    context.Response.ContentLength = stm.Length;

                    await stm.CopyToAsync(mst);
                    await context.Response.WriteAsync(mst.ToArray());

                    return;
                }
            }

            // handle the request
            var o = await HandleRequest(context);
            if (o == null)
                return;

            // handle the response
            foreach (var p in responseProcessors.OrderByDescending(i => i.Metadata.Infos.Max(j => j.Priority)))
                if (p.Metadata.Infos.Any(i => i.Type.IsInstanceOfType(o)))
                    if ((await p.Value.Handle(context, o)))
                        return;

            return;
        }

        /// <summary>
        /// Handles a given request and returns the result.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<object> HandleRequest(IContext context)
        {
            switch (context.Request.Method)
            {
                case "GET":
                    return GetRequest(context, context.Request.Path.Value);
                case "PUT":
                    return PutRequest(context, context.Request.Path.Value);
            }

            return null;
        }

        /// <summary>
        /// Implements a GET request.
        /// </summary>
        /// <returns></returns>
        public async Task<object> GetRequest(IContext context, string uri)
        {
            // split and clean up uri
            var path = uri != null ? uri.Split('/')
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .ToArray() : new string[0];

            // resolve the path into an object
            var o = await Resolve(context, bus, path) ?? HttpStatusCode.NotFound;
            if (o is HttpStatusCode ||
                o is Response)
                return o;

            // handle GET request
            return await InvokeGet(context, o) ?? HttpStatusCode.MethodNotAllowed;
        }

        /// <summary>
        /// Implements a PUT request.
        /// </summary>
        /// <returns></returns>
        public async Task<object> PutRequest(IContext context, string uri)
        {
            // split and clean up uri
            var path = uri.Split('/')
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .ToArray();

            // resolve the path into an object
            var o = await Resolve(context, bus, path);
            if (o == null)
                return HttpStatusCode.NotFound;

            // handle GET request
            return await InvokePut(context, o) ?? HttpStatusCode.NoContent;
        }

        /// <summary>
        /// Fully resolves the path from the given object.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        async Task<object> Resolve(IContext context, object source, string[] path)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(path != null);

            var o = await InvokeResolve(context, source, path);
            while (o is ResolveResponse)
                o = await InvokeResolve(context, ((ResolveResponse)o).Object, ((ResolveResponse)o).Path);

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
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(invoke != null);

            var o = source;

            foreach (var p in requestProcessors.OrderByDescending(i => i.Metadata.Infos.Max(j => j.Priority)))
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
        Task<object> InvokeResolve(IContext context, object source, string[] path)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(path != null);

            return Invoke(source, i => i.Resolve(context, source, path));
        }

        /// <summary>
        /// Invokes the request processors in order to handle a GET request.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<object> InvokeGet(IContext context, object source)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            return Invoke(source, i => i.Get(context, source));
        }

        /// <summary>
        /// Invokes the request processors in order to handle a PUT request.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<object> InvokePut(IContext context, object source)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            return Invoke(source, i => i.Put(context, source));
        }

        /// <summary>
        /// Searches the given set of <see cref="RnetBusObject"/>s for the one with the matching ID.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        internal async Task<RnetBusObject> FindObject(IEnumerable<RnetBusObject> source, string id)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(id != null);

            // find first matching ID
            foreach (var o in source)
            {
                var i = await o.GetId(profileManager);
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
        public async Task<ObjectData> ObjectToData(IContext context, RnetBusObject d)
        {
            Contract.Requires<ArgumentNullException>(d != null);

            if (d is RnetDevice)
                return await DeviceToData(context, (RnetDevice)d);
            else
                return await FillObjectData(context, d, new ObjectData());
        }

        async Task<ObjectData> FillObjectData(IContext context, RnetBusObject o, ObjectData d)
        {
            Contract.Requires<ArgumentNullException>(o != null);
            Contract.Requires<ArgumentNullException>(d != null);

            d.Uri = await o.GetUri(profileManager, context);
            d.FriendlyUri = await o.GetFriendlyUri(profileManager, context);
            d.Id = await o.GetId(profileManager);
            d.Name = await o.GetName(profileManager, context);
            d.Objects = await GetObjects(context, o);
            d.Profiles = await GetProfileRefs(context, o);
            return d;
        }

        /// <summary>
        /// Transforms the given <see cref="RnetDevice"/> into a <see cref="DeviceData"/> instance.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public async Task<DeviceData> DeviceToData(IContext context, RnetDevice d)
        {
            Contract.Requires<ArgumentNullException>(d != null);

            if (d is RnetController)
                return await ControllerToData(context, (RnetController)d);
            else
                return await FillDeviceData(context, d, new DeviceData());
        }

        async Task<DeviceData> FillDeviceData(IContext context, RnetDevice o, DeviceData d)
        {
            Contract.Requires<ArgumentNullException>(o != null);
            Contract.Requires<ArgumentNullException>(d != null);

            await FillObjectData(context, o, d);
            d.RnetId = o.GetId();
            d.DataUri = o.GetUri(context).UriCombine(Util.DATA_URI_SEGMENT);
            return d;
        }

        /// <summary>
        /// Transforms the given <see cref="RnetController"/> into a <see cref="ControllerData"/> instance.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public async Task<ControllerData> ControllerToData(IContext context, RnetController d)
        {
            return await FillControllerData(context, d, new ControllerData());
        }

        async Task<ControllerData> FillControllerData(IContext context, RnetController o, ControllerData d)
        {
            await FillDeviceData(context, o, d);
            return d;
        }

        /// <summary>
        /// Gets the child objects refs for the given object.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public async Task<ObjectDataCollection> GetObjects(IContext context, RnetBusObject o)
        {
            Contract.Requires<ArgumentNullException>(o != null);

            // load container
            var p = await profileManager.GetProfile<IContainer>(o) ?? Enumerable.Empty<RnetBusObject>();
            return new ObjectDataCollection(await Task.WhenAll(p.Select(i => ObjectToData(context, i))));
        }

        /// <summary>
        /// Gets the profile refs for the given object.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public async Task<ProfileRefCollection> GetProfileRefs(IContext context, RnetBusObject o)
        {
            Contract.Requires<ArgumentNullException>(o != null);

            // load container
            var p = await profileManager.GetProfiles(o) ?? Enumerable.Empty<ProfileHandle>();
            return new ProfileRefCollection(await Task.WhenAll(p.Select(i => ProfileToRef(context, i))));
        }

        public async Task<ProfileRef> ProfileToRef(IContext context, ProfileHandle profile)
        {
            return new ProfileRef()
            {
                Uri = await profile.GetUri(profileManager, context),
                FriendlyUri = await profile.GetFriendlyUri(profileManager, context),
                Id = profile.Metadata.Id,
            };
        }

    }

}
