using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

using Nancy;

using Rnet.Drivers;
using Rnet.Service.Processors;

namespace Rnet.Service.Objects
{

    [Export(typeof(IRequestProcessor))]
    [RequestProcessorMultiple(typeof(ProfileHandle))]
    [RequestProcessorMultiple(typeof(ProfilePropertyHandle))]
    [RequestProcessorMultiple(typeof(ProfileCommandHandle))]
    public class ProfileRequestProcessor : IRequestProcessor
    {

        BusModule module;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="module"></param>
        [ImportingConstructor]
        public ProfileRequestProcessor(
            BusModule module)
        {
            Contract.Requires<ArgumentNullException>(module != null);

            this.module = module;
        }

        NancyContext Context
        {
            get { return module.Context; }
        }

        public Task<object> Resolve(object target, string[] path)
        {
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
            // search for property
            var property = profile.Metadata.Properties.FirstOrDefault(i => i.Name == path[0]);
            if (property != null)
                return Task.FromResult<object>(new ResolveResponse(profile[property], path.Skip(1).ToArray()));

            // search for command
            var command = profile.Metadata.Commands.FirstOrDefault(i => i.Name == path[0]);
            if (command != null)
                return Task.FromResult<object>(new ResolveResponse(profile[property], path.Skip(1).ToArray()));

            return Task.FromResult<object>(null);
        }

        Task<object> Resolve(ProfilePropertyHandle property, string[] path)
        {
            return Task.FromResult<object>(null);
            //return Task.FromResult<object>(new ResolveResponse(property, path.Skip(1).ToArray()));
        }

        Task<object> Resolve(ProfileCommandHandle command, string[] path)
        {
            return Task.FromResult<object>(null);
            //return Task.FromResult<object>(new ResolveResponse(command, path.Skip(1).ToArray()));
        }

        public Task<object> Get(object target)
        {
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
            return new ProfileData()
            {
                Uri = await profile.GetUri(Context),
                FriendlyUri = await profile.GetFriendlyUri(Context),
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
            return await PropertyToData(property);
        }

        async Task<ProfilePropertyDataCollection> GetProperties(ProfileHandle profile)
        {
            return new ProfilePropertyDataCollection(await Task.WhenAll(
                profile.Metadata.Properties
                    .Select(async i =>
                        await PropertyToData(profile[i]))));
        }

        async Task<ProfilePropertyData> PropertyToData(ProfilePropertyHandle property)
        {
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
            return (await property.Profile.GetUri(Context)).UriCombine(property.Metadata.Name);
        }

        async Task<Uri> GetPropertyFriendlyUri(ProfilePropertyHandle property)
        {
            return (await property.Profile.GetFriendlyUri(Context)).UriCombine(property.Metadata.Name);
        }

        async Task<object> Get(ProfileCommandHandle command)
        {
            return await CommandToData(command);
        }

        async Task<ProfileCommandDataCollection> GetCommands(ProfileHandle profile)
        {
            return new ProfileCommandDataCollection(await Task.WhenAll(
                profile.Metadata.Commands
                    .Select(async i =>
                        await CommandToData(profile[i]))));
        }

        async Task<ProfileCommandData> CommandToData(ProfileCommandHandle command)
        {
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
            return (await command.Profile.GetUri(Context)).UriCombine(command.Metadata.Name);
        }

        async Task<Uri> GetCommandFriendlyUri(ProfileCommandHandle command)
        {
            return (await command.Profile.GetFriendlyUri(Context)).UriCombine(command.Metadata.Name);
        }

        public Task<object> Put(object target)
        {
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
            return Task.FromResult<object>(HttpStatusCode.NotImplemented);
        }

        Task<object> Put(ProfilePropertyHandle property)
        {
            return Task.FromResult<object>(HttpStatusCode.NotImplemented);
        }

        Task<object> Put(ProfileCommandHandle property)
        {
            return Task.FromResult<object>(HttpStatusCode.NotImplemented);
        }

    }

}
