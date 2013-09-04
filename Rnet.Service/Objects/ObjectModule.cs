using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

using LinqToAwait;

using Nancy;

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
    public sealed class ObjectModule : NancyModuleBase
    {

        IEnumerable<Lazy<IRequestProcessor>> processors;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="bus"></param>
        [ImportingConstructor]
        public ObjectModule(
            [Import] RnetBus bus,
            [ImportMany] IEnumerable<Lazy<IRequestProcessor>> processors)
            : base(bus, "/objects")
        {
            Contract.Requires<ArgumentNullException>(bus != null);
            Contract.Requires<ArgumentNullException>(processors != null);

            this.processors = processors;

            Get["/", true] = async (x, ct) => await GetRequest("");
            Get["/{Uri*}", true] = async (x, ct) => await GetRequest(x.Uri);
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

            // process request
            return await processors.ToObservable()
                .WhereAsync(i => i.Value.CanProcess(Context, "GET", uri, o))
                .SelectAsync(i => i.Value.Process(Context, "GET", uri, o))
                .Where(i => i != null)
                .FirstOrDefaultAsync();
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
                return bus.Controllers;

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

    }

}
