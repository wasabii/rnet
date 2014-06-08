using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Owin;
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

        public Task<object> Resolve(IOwinContext context, object target, string[] path)
        {
            // end of path
            if (path.Length == 0)
                return Task.FromResult<object>(target);

            if (target is ProfileHandle)
                return Resolve(context, (ProfileHandle)target, path);

            if (target is ProfilePropertyHandle)
                return Resolve(context, (ProfilePropertyHandle)target, path);

            if (target is ProfileCommandHandle)
                return Resolve(context, (ProfileCommandHandle)target, path);

            return null;
        }

        Task<object> Resolve(IOwinContext context, ProfileHandle profile, string[] path)
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

        Task<object> Resolve(IOwinContext context, ProfilePropertyHandle property, string[] path)
        {
            Contract.Requires<ArgumentNullException>(property != null);
            Contract.Requires<ArgumentNullException>(path != null);

            return Task.FromResult<object>(null);
            //return Task.FromResult<object>(new ResolveResponse(property, path.Skip(1).ToArray()));
        }

        Task<object> Resolve(IOwinContext context, ProfileCommandHandle command, string[] path)
        {
            Contract.Requires<ArgumentNullException>(command != null);
            Contract.Requires<ArgumentNullException>(path != null);

            return Task.FromResult<object>(null);
            //return Task.FromResult<object>(new ResolveResponse(command, path.Skip(1).ToArray()));
        }

        public Task<object> Get(IOwinContext context, object target)
        {
            var t = module.Bind(context);

            if (target is ProfileHandle)
                return Get(context, (ProfileHandle)target);

            if (target is ProfilePropertyHandle)
                return Get(context, (ProfilePropertyHandle)target);

            if (target is ProfileCommandHandle)
                return Get(context, (ProfileCommandHandle)target);

            return null;
        }

        async Task<object> Get(IOwinContext context, ProfileHandle profile)
        {
            Contract.Requires<ArgumentNullException>(profile != null);

            return new ProfileData()
            {
                Uri = await profile.GetUri(profileManager, context),
                FriendlyUri = await profile.GetFriendlyUri(profileManager, context),
                Id = profile.Metadata.Id,
                Name = profile.Metadata.Name,
                Namespace = profile.Metadata.Namespace,
                XmlNamespace = profile.Metadata.XmlNamespace.NamespaceName,
                Properties = await GetProperties(context, profile),
                Commands = await GetCommands(context, profile),
            };
        }

        async Task<object> Get(IOwinContext context, ProfilePropertyHandle property)
        {
            Contract.Requires<ArgumentNullException>(property != null);

            dynamic set = null;

            var data = module.Bind<ProfilePropertyData>(context);
            if (data != null)
                if (data.Value != null)
                    set = data.Value;

            if (context.Request.Query != null)
                set = context.Request.Query;

            if (set != null)
                if (set.GetType() == property.Metadata.Type)
                    property.Set(set);
                else
                    property.Set(Convert.ChangeType(set, property.Metadata.Type));

            return await PropertyToData(context, property);
        }

        async Task<ProfilePropertyDataCollection> GetProperties(IOwinContext context, ProfileHandle profile)
        {
            Contract.Requires<ArgumentNullException>(profile != null);

            return new ProfilePropertyDataCollection(await Task.WhenAll(
                profile.Metadata.Properties
                    .Select(async i =>
                        await PropertyToData(context, profile[i]))));
        }

        async Task<ProfilePropertyData> PropertyToData(IOwinContext context, ProfilePropertyHandle property)
        {
            Contract.Requires<ArgumentNullException>(property != null);

            return new ProfilePropertyData()
            {
                Uri = await GetPropertyUri(context, property),
                FriendlyUri = await GetPropertyFriendlyUri(context, property),
                Name = property.Metadata.Name,
                XmlNamespace = property.Profile.Metadata.XmlNamespace.NamespaceName,
                Value = property.Get(),
            };
        }

        async Task<Uri> GetPropertyUri(IOwinContext context, ProfilePropertyHandle property)
        {
            Contract.Requires<ArgumentNullException>(property != null);

            return (await property.Profile.GetUri(profileManager, context)).UriCombine(property.Metadata.Name);
        }

        async Task<Uri> GetPropertyFriendlyUri(IOwinContext context, ProfilePropertyHandle property)
        {
            Contract.Requires<ArgumentNullException>(property != null);

            return (await property.Profile.GetFriendlyUri(profileManager, context)).UriCombine(property.Metadata.Name);
        }

        async Task<object> Get(IOwinContext context, ProfileCommandHandle command)
        {
            Contract.Requires<ArgumentNullException>(command != null);

            return await CommandToData(context, command);
        }

        async Task<ProfileCommandDataCollection> GetCommands(IOwinContext context, ProfileHandle profile)
        {
            Contract.Requires<ArgumentNullException>(profile != null);

            return new ProfileCommandDataCollection(await Task.WhenAll(
                profile.Metadata.Commands
                    .Select(async i =>
                        await CommandToData(context, profile[i]))));
        }

        async Task<ProfileCommandData> CommandToData(IOwinContext context, ProfileCommandHandle command)
        {
            Contract.Requires<ArgumentNullException>(command != null);

            return new ProfileCommandData()
            {
                Uri = await GetCommandUri(context, command),
                FriendlyUri = await GetCommandFriendlyUri(context, command),
                Name = command.Metadata.Name,
                XmlNamespace = command.Profile.Metadata.XmlNamespace.NamespaceName,
            };
        }

        async Task<Uri> GetCommandUri(IOwinContext context, ProfileCommandHandle command)
        {
            Contract.Requires<ArgumentNullException>(command != null);

            return (await command.Profile.GetUri(profileManager, context)).UriCombine(command.Metadata.Name);
        }

        async Task<Uri> GetCommandFriendlyUri(IOwinContext context, ProfileCommandHandle command)
        {
            Contract.Requires<ArgumentNullException>(command != null);

            return (await command.Profile.GetFriendlyUri(profileManager, context)).UriCombine(command.Metadata.Name);
        }

        public Task<object> Put(IOwinContext context, object target)
        {
            if (target is ProfileHandle)
                return Put(context, (ProfileHandle)target);

            if (target is ProfilePropertyHandle)
                return Put(context, (ProfilePropertyHandle)target);

            if (target is ProfileCommandHandle)
                return Put(context, (ProfileCommandHandle)target);

            return Task.FromResult<object>(HttpStatusCode.MethodNotAllowed);
        }

        Task<object> Put(IOwinContext context, ProfileHandle profile)
        {
            Contract.Requires<ArgumentNullException>(profile != null);

            return Task.FromResult<object>(HttpStatusCode.NotImplemented);
        }

        Task<object> Put(IOwinContext context, ProfilePropertyHandle property)
        {
            Contract.Requires<ArgumentNullException>(property != null);

            var t = module.Bind<XDocument>(context);
            return Task.FromResult<object>(HttpStatusCode.NotImplemented);
        }

        Task<object> Put(IOwinContext context, ProfileCommandHandle property)
        {
            Contract.Requires<ArgumentNullException>(property != null);

            return Task.FromResult<object>(HttpStatusCode.NotImplemented);
        }

    }

}
