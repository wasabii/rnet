using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Rnet.Drivers;
using Rnet.Profiles.Core;
using Rnet.Service.Host.Models;

namespace Rnet.Service.Host.Processors
{

    /// <summary>
    /// Serves as a base type for other <see cref="ObjectRequestProcessor"/> types.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [RequestProcessor(typeof(RnetBusObject))]
    public sealed class ObjectRequestProcessor :
        ObjectRequestProcessor<RnetBusObject>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="root"></param>
        [ImportingConstructor]
        public ObjectRequestProcessor(
            RootProcessor root,
            ProfileManager profileManager)
            : base(root, profileManager)
        {
            Contract.Requires<ArgumentNullException>(root != null);
            Contract.Requires<ArgumentNullException>(profileManager != null);
        }

    }

    /// <summary>
    /// Handles requests for <see cref="RnetBusObject"/> instances.
    /// </summary>
    public abstract class ObjectRequestProcessor<T> :
        RequestProcessor<T>
        where T : RnetBusObject
    {

        readonly ProfileManager profileManager;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        protected ObjectRequestProcessor(
            RootProcessor root,
            ProfileManager profileManager)
            : base(root)
        {
            Contract.Requires<ArgumentNullException>(root != null);

            this.profileManager = profileManager;
        }

        public override async Task<object> Resolve(IContext context, T target, string[] path)
        {
            // referring to a profile
            if (path[0].StartsWith(Util.PROFILE_URI_PREFIX))
                return await ResolveProfile(context, target, path, path[0].Substring(Util.PROFILE_URI_PREFIX.Length));

            // object contains other objects
            var c = await profileManager.GetProfile<IContainer>(target);
            if (c != null)
            {
                // find contained object with specified id
                var o = await Root.FindObject(c, path[0]);
                if (o != null)
                    return new ResolveResponse(o, path.Skip(1).ToArray());
            }

            return null;
        }

        /// <summary>
        /// Resolves the given profile.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="path"></param>
        /// <param name="profileId"></param>
        /// <returns></returns>
        async Task<object> ResolveProfile(IContext context, RnetBusObject target, string[] path, string profileId)
        {
            Contract.Requires<ArgumentNullException>(target != null);
            Contract.Requires<ArgumentException>(path != null && path.Length > 0);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(profileId));

            // find matching profile
            var profiles = await profileManager.GetProfiles(target);
            if (profiles == null)
                return null;

            // first profile with metadata that corresponds with uri
            var profile = profiles.FirstOrDefault(i => i.Metadata.Id == profileId);
            if (profile != null)
                return new ResolveResponse(profile, path.Skip(1).ToArray());

            return null;
        }

        public override async Task<object> Get(IContext context, T target)
        {
            return await ObjectToData(context, target);
        }

        public override Task<object> Put(IContext context, T target)
        {
            return Task.FromResult<object>(HttpStatusCode.NotImplemented);
        }

        #region Transform To Model

        /// <summary>
        /// Transforms the given <see cref="RnetBusObject"/> into a <see cref="ObjectData"/> instance.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        protected async Task<ObjectData> ObjectToData(IContext context, RnetBusObject o)
        {
            Contract.Requires<ArgumentNullException>(o != null);

            if (o is RnetDevice)
                return await DeviceToData(context, (RnetDevice)o);

            return await FillObjectData(context, o, new ObjectData());
        }

        /// <summary>
        /// Populates the <see cref="ObjectData"/> model.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="d"></param>
        /// <returns></returns>
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
        /// <param name="o"></param>
        /// <returns></returns>
        protected async Task<DeviceData> DeviceToData(IContext context, RnetDevice o)
        {
            Contract.Requires<ArgumentNullException>(o != null);

            if (o is RnetController)
                return await ControllerToData(context, (RnetController)o);

            return await FillDeviceData(context, o, new DeviceData());
        }

        /// <summary>
        /// Populates the <see cref="DeviceData"/> model.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="d"></param>
        /// <returns></returns>
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
        async Task<ControllerData> ControllerToData(IContext context, RnetController d)
        {
            Contract.Requires<ArgumentNullException>(d != null);

            return await FillControllerData(context, d, new ControllerData());
        }

        /// <summary>
        /// Populates the <see cref="ControllerData"/> model.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        async Task<ControllerData> FillControllerData(IContext context, RnetController o, ControllerData d)
        {
            Contract.Requires<ArgumentNullException>(o != null);
            Contract.Requires<ArgumentNullException>(d != null);

            await FillDeviceData(context, o, d);
            return d;
        }

        /// <summary>
        /// Transforms all available objects on the <see cref="RnetBusObject"/> into model instances.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        async Task<ObjectDataCollection> GetObjects(IContext context, RnetBusObject o)
        {
            Contract.Requires<ArgumentNullException>(o != null);

            // load container
            var p = await profileManager.GetProfile<IContainer>(o) ?? Enumerable.Empty<RnetBusObject>();
            return new ObjectDataCollection(await Task.WhenAll(p.Select(i => ObjectToData(context, i))));
        }

        /// <summary>
        /// Transforms all available profiles on the <see cref="RnetBusObject"/> into model instances.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        async Task<ProfileRefCollection> GetProfileRefs(IContext context, RnetBusObject o)
        {
            Contract.Requires<ArgumentNullException>(o != null);

            // load container
            var p = await profileManager.GetProfiles(o) ?? Enumerable.Empty<ProfileHandle>();
            return new ProfileRefCollection(await Task.WhenAll(p.Select(i => ProfileToRef(context, i))));
        }

        /// <summary>
        /// Transforms a <see cref="ProfileHandle"/> into a <see cref="ProfileRef"/> model instance.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        async Task<ProfileRef> ProfileToRef(IContext context, ProfileHandle profile)
        {
            Contract.Requires<ArgumentNullException>(profile != null);

            return new ProfileRef()
            {
                Uri = await profile.GetUri(profileManager, context),
                FriendlyUri = await profile.GetFriendlyUri(profileManager, context),
                Id = profile.Metadata.Id,
            };
        }

        #endregion

    }

}
