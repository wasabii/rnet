using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using Nancy;
using Nancy.ModelBinding;

using Rnet.Drivers;
using Rnet.Service.Host.Models;

namespace Rnet.Service.Host.Processors
{

    [Export(typeof(IRequestProcessor))]
    [RequestProcessorMultiple(typeof(ProfileHandle))]
    [RequestProcessorMultiple(typeof(ProfilePropertyHandle))]
    [RequestProcessorMultiple(typeof(ProfileCommandHandle))]
    public class ProfileRequestProcessor : IRequestProcessor
    {

        readonly BusModule module;
        readonly ProfileManager profileManager;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="module"></param>
        [ImportingConstructor]
        public ProfileRequestProcessor(
            BusModule module,
            ProfileManager profileManager)
        {
            Contract.Requires<ArgumentNullException>(module != null);
            Contract.Requires<ArgumentNullException>(profileManager != null);

            this.module = module;
            this.profileManager = profileManager;
        }

        NancyContext Context
        {
            get { return module.Context; }
        }

        public Task<object> Resolve(object target, string[] path)
        {
            Contract.Requires<ArgumentNullException>(target != null);
            Contract.Requires<ArgumentNullException>(path != null);

            // end of path
            if (path.Length == 0)
                return Task.FromResult<object>(target);

            if (target is ProfileHandle)
                return Resolve((ProfileHandle)target, path);

            if (target is ProfilePropertyHandle)
                return Resolve((ProfilePropertyHandle)target, path);

            if (target is ProfileCommandHandle)
                return Resolve((ProfileCommandHandle)target, path);

            return null;
        }

        Task<object> Resolve(ProfileHandle profile, string[] path)
        {
            Contract.Requires<ArgumentNullException>(profile != null);
            Contract.Requires<ArgumentNullException>(path != null);

            // search for property
            var property = profile.Metadata.Properties.FirstOrDefault(i => i.Name == path[0]);
            if (property != null)
                return Task.FromResult<object>(new ResolveResponse(profile[property], path.Skip(1).ToArray()));

            // search for command
            var command = profile.Metadata.Commands.FirstOrDefault(i => i.Name == path[0]);
            if (command != null)
                return Task.FromResult<object>(new ResolveResponse(profile[command], path.Skip(1).ToArray()));

            return Task.FromResult<object>(null);
        }

        Task<object> Resolve(ProfilePropertyHandle property, string[] path)
        {
            Contract.Requires<ArgumentNullException>(property != null);
            Contract.Requires<ArgumentNullException>(path != null);

            return Task.FromResult<object>(null);
            //return Task.FromResult<object>(new ResolveResponse(property, path.Skip(1).ToArray()));
        }

        Task<object> Resolve(ProfileCommandHandle command, string[] path)
        {
            Contract.Requires<ArgumentNullException>(command != null);
            Contract.Requires<ArgumentNullException>(path != null);

            return Task.FromResult<object>(null);
            //return Task.FromResult<object>(new ResolveResponse(command, path.Skip(1).ToArray()));
        }

        public Task<object> Get(object target)
        {
            Contract.Requires<ArgumentNullException>(target != null);

            var t = module.Bind();

            if (target is ProfileHandle)
                return Get((ProfileHandle)target);

            if (target is ProfilePropertyHandle)
                return Get((ProfilePropertyHandle)target);

            if (target is ProfileCommandHandle)
                return Get((ProfileCommandHandle)target);

            return null;
        }

        async Task<object> Get(ProfileHandle profile)
        {
            Contract.Requires<ArgumentNullException>(profile != null);

            return new ProfileData()
            {
                Uri = await profile.GetUri(profileManager, Context),
                FriendlyUri = await profile.GetFriendlyUri(profileManager, Context),
                Id = profile.Metadata.Id,
                Name = profile.Metadata.Name,
                Namespace = profile.Metadata.Namespace,
                XmlNamespace = profile.Metadata.XmlNamespace.NamespaceName,
                Properties = await GetProperties(profile),
                Commands = await GetCommands(profile),
            };
        }

        async Task<object> Get(ProfilePropertyHandle property)
        {
            Contract.Requires<ArgumentNullException>(property != null);

            dynamic set = null;

            var data = module.Bind<ProfilePropertyData>();
            if (data != null)
                if (data.Value != null)
                    set = data.Value;

            if (module.Request.Query.Value != null)
                set = module.Request.Query.Value;

            if (set != null)
                if (set.GetType() == property.Metadata.Type)
                    property.Set(set);
                else
                    property.Set(Convert.ChangeType(set, property.Metadata.Type));

            return await PropertyToData(property);
        }

        async Task<ProfilePropertyDataCollection> GetProperties(ProfileHandle profile)
        {
            Contract.Requires<ArgumentNullException>(profile != null);

            return new ProfilePropertyDataCollection(await Task.WhenAll(
                profile.Metadata.Properties
                    .Select(async i =>
                        await PropertyToData(profile[i]))));
        }

        async Task<ProfilePropertyData> PropertyToData(ProfilePropertyHandle property)
        {
            Contract.Requires<ArgumentNullException>(property != null);

            return new ProfilePropertyData()
            {
                Uri = await GetPropertyUri(property),
                FriendlyUri = await GetPropertyFriendlyUri(property),
                Name = property.Metadata.Name,
                XmlNamespace = property.Profile.Metadata.XmlNamespace.NamespaceName,
                Value = property.Get(),
            };
        }

        async Task<Uri> GetPropertyUri(ProfilePropertyHandle property)
        {
            Contract.Requires<ArgumentNullException>(property != null);

            return (await property.Profile.GetUri(profileManager, Context)).UriCombine(property.Metadata.Name);
        }

        async Task<Uri> GetPropertyFriendlyUri(ProfilePropertyHandle property)
        {
            Contract.Requires<ArgumentNullException>(property != null);

            return (await property.Profile.GetFriendlyUri(profileManager, Context)).UriCombine(property.Metadata.Name);
        }

        async Task<object> Get(ProfileCommandHandle command)
        {
            Contract.Requires<ArgumentNullException>(command != null);

            return await CommandToData(command);
        }

        async Task<ProfileCommandDataCollection> GetCommands(ProfileHandle profile)
        {
            Contract.Requires<ArgumentNullException>(profile != null);

            return new ProfileCommandDataCollection(await Task.WhenAll(
                profile.Metadata.Commands
                    .Select(async i =>
                        await CommandToData(profile[i]))));
        }

        async Task<ProfileCommandData> CommandToData(ProfileCommandHandle command)
        {
            Contract.Requires<ArgumentNullException>(command != null);

            return new ProfileCommandData()
            {
                Uri = await GetCommandUri(command),
                FriendlyUri = await GetCommandFriendlyUri(command),
                Name = command.Metadata.Name,
                XmlNamespace = command.Profile.Metadata.XmlNamespace.NamespaceName,
            };
        }

        async Task<Uri> GetCommandUri(ProfileCommandHandle command)
        {
            Contract.Requires<ArgumentNullException>(command != null);

            return (await command.Profile.GetUri(profileManager, Context)).UriCombine(command.Metadata.Name);
        }

        async Task<Uri> GetCommandFriendlyUri(ProfileCommandHandle command)
        {
            Contract.Requires<ArgumentNullException>(command != null);

            return (await command.Profile.GetFriendlyUri(profileManager, Context)).UriCombine(command.Metadata.Name);
        }

        public Task<object> Put(object target)
        {
            Contract.Requires<ArgumentNullException>(target != null);

            if (target is ProfileHandle)
                return Put((ProfileHandle)target);

            if (target is ProfilePropertyHandle)
                return Put((ProfilePropertyHandle)target);

            if (target is ProfileCommandHandle)
                return Put((ProfileCommandHandle)target);

            return Task.FromResult<object>(HttpStatusCode.MethodNotAllowed);
        }

        Task<object> Put(ProfileHandle profile)
        {
            Contract.Requires<ArgumentNullException>(profile != null);

            return Task.FromResult<object>(HttpStatusCode.NotImplemented);
        }

        Task<object> Put(ProfilePropertyHandle property)
        {
            Contract.Requires<ArgumentNullException>(property != null);

            var t = module.Bind<XDocument>();
            return Task.FromResult<object>(HttpStatusCode.NotImplemented);
        }

        Task<object> Put(ProfileCommandHandle property)
        {
            Contract.Requires<ArgumentNullException>(property != null);

            return Task.FromResult<object>(HttpStatusCode.NotImplemented);
        }

    }

}
