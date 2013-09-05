using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Rnet.Drivers;
using Rnet.Profiles.Core;
using Rnet.Service.Processors;

namespace Rnet.Service.Objects
{

    /// <summary>
    /// Handles requests for <see cref="RnetBusObject"/> instances.
    /// </summary>
    [RequestProcessor(typeof(RnetBusObject), -100)]
    public class ObjectRequestProcessor : RequestProcessor<RnetBusObject>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        [ImportingConstructor]
        protected ObjectRequestProcessor(
            BusModule module)
            : base(module)
        {
            Contract.Requires<ArgumentNullException>(module != null);
        }

        public override async Task<object> Resolve(RnetBusObject target, string[] path)
        {
            // referring to a profile
            if (path[0].StartsWith(Util.PROFILE_URI_PREFIX))
                return await ResolveProfile(target, path, path[0].Substring(Util.PROFILE_URI_PREFIX.Length));

            // object contains other objects
            var c = await target.GetProfile<IContainer>();
            if (c != null)
            {
                // find contained object with specified id
                var o = await Module.FindObject(c, path[0]);
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
        async Task<object> ResolveProfile(RnetBusObject target, string[] path, string profileId)
        {
            Contract.Requires<ArgumentNullException>(target != null);
            Contract.Requires<ArgumentNullException>(path != null);
            Contract.Requires<ArgumentNullException>(profileId != null);

            // find matching profile
            var profiles = await target.GetProfiles();
            if (profiles == null)
                return null;

            // first profile with metadata that corresponds with uri
            var profile = profiles.FirstOrDefault(i => i.Metadata.Id == profileId);
            if (profile != null)
                return new ResolveResponse(profile, path.Skip(1).ToArray());

            return null;
        }

        public override async Task<object> Get(RnetBusObject target)
        {
            return await Module.ObjectToData(target);
        }

        public override Task<object> Put(RnetBusObject target)
        {
            throw new NotImplementedException();
        }

    }

}
