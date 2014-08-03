using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Rnet.Drivers;
using Rnet.Service.Host.Models;
using Rnet.Service.Host.Net;
using Rnet.Service.Host.Serialization;

namespace Rnet.Service.Host.Processors
{

    /// <summary>
    /// Handles requests against a profile object.
    /// </summary>
    [Export(typeof(IRequestProcessor))]
    [RequestProcessorMultiple(typeof(ProfileHandle))]
    [RequestProcessorMultiple(typeof(ProfilePropertyHandle))]
    [RequestProcessorMultiple(typeof(ProfileCommandHandle))]
    public class ProfileRequestProcessor :
        IRequestProcessor
    {

        readonly RootProcessor module;
        readonly ProfileManager profileManager;
        readonly IEnumerable<IBodyDeserializer> serializers;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="module"></param>
        /// <param name="profileManager"></param>
        /// <param name="serializers"></param>
        [ImportingConstructor]
        public ProfileRequestProcessor(
            RootProcessor module,
            ProfileManager profileManager,
            [ImportMany] IEnumerable<IBodyDeserializer> serializers)
        {
            Contract.Requires<ArgumentNullException>(module != null);
            Contract.Requires<ArgumentNullException>(profileManager != null);
            Contract.Requires<ArgumentNullException>(serializers != null);

            this.module = module;
            this.profileManager = profileManager;
            this.serializers = serializers;
        }

        public Task<object> Resolve(IContext context, object target, string[] path)
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

        Task<object> Resolve(IContext context, ProfileHandle profile, string[] path)
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

        Task<object> Resolve(IContext context, ProfilePropertyHandle property, string[] path)
        {
            Contract.Requires<ArgumentNullException>(property != null);
            Contract.Requires<ArgumentNullException>(path != null);

            return Task.FromResult<object>(null);
        }

        Task<object> Resolve(IContext context, ProfileCommandHandle command, string[] path)
        {
            Contract.Requires<ArgumentNullException>(command != null);
            Contract.Requires<ArgumentNullException>(path != null);

            return Task.FromResult<object>(null);
        }

        public Task<object> Get(IContext context, object target)
        {
            if (target is ProfileHandle)
                return Get(context, (ProfileHandle)target);

            if (target is ProfilePropertyHandle)
                return Get(context, (ProfilePropertyHandle)target);

            if (target is ProfileCommandHandle)
                return Get(context, (ProfileCommandHandle)target);

            return null;
        }

        async Task<object> Get(IContext context, ProfileHandle profile)
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

        async Task<object> Get(IContext context, ProfilePropertyHandle property)
        {
            Contract.Requires<ArgumentNullException>(property != null);

            // set from query string
            if (context.Request.Query != null)
            {
                var str = context.Request.Query["Value"];
                if (str != null)
                {
                    var conv = TypeDescriptor.GetConverter(property.Metadata.Type);
                    if (conv != null && conv.CanConvertFrom(typeof(string)))
                    {
                        var value = conv.ConvertFromString(str);
                        if (value != null)
                            property.Set(value);
                    }
                }
            }

            return await PropertyToData(context, property);
        }

        async Task<ProfilePropertyDataCollection> GetProperties(IContext context, ProfileHandle profile)
        {
            Contract.Requires<ArgumentNullException>(profile != null);

            return new ProfilePropertyDataCollection(await Task.WhenAll(
                profile.Metadata.Properties
                    .Select(async i =>
                        await PropertyToData(context, profile[i]))));
        }

        async Task<ProfilePropertyData> PropertyToData(IContext context, ProfilePropertyHandle property)
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

        async Task<Uri> GetPropertyUri(IContext context, ProfilePropertyHandle property)
        {
            Contract.Requires<ArgumentNullException>(property != null);

            return (await property.Profile.GetUri(profileManager, context)).UriCombine(property.Metadata.Name);
        }

        async Task<Uri> GetPropertyFriendlyUri(IContext context, ProfilePropertyHandle property)
        {
            Contract.Requires<ArgumentNullException>(property != null);

            return (await property.Profile.GetFriendlyUri(profileManager, context)).UriCombine(property.Metadata.Name);
        }

        async Task<object> Get(IContext context, ProfileCommandHandle command)
        {
            Contract.Requires<ArgumentNullException>(command != null);

            var exec = context.Request.QueryString.Value;
            if (exec == "Execute")
                command.Invoke();

            return await CommandToData(context, command);
        }

        async Task<ProfileCommandDataCollection> GetCommands(IContext context, ProfileHandle profile)
        {
            Contract.Requires<ArgumentNullException>(profile != null);

            return new ProfileCommandDataCollection(await Task.WhenAll(
                profile.Metadata.Commands
                    .Select(async i =>
                        await CommandToData(context, profile[i]))));
        }

        async Task<ProfileCommandData> CommandToData(IContext context, ProfileCommandHandle command)
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

        async Task<Uri> GetCommandUri(IContext context, ProfileCommandHandle command)
        {
            Contract.Requires<ArgumentNullException>(command != null);

            return (await command.Profile.GetUri(profileManager, context)).UriCombine(command.Metadata.Name);
        }

        async Task<Uri> GetCommandFriendlyUri(IContext context, ProfileCommandHandle command)
        {
            Contract.Requires<ArgumentNullException>(command != null);

            return (await command.Profile.GetFriendlyUri(profileManager, context)).UriCombine(command.Metadata.Name);
        }

        public Task<object> Put(IContext context, object target)
        {
            if (target is ProfileHandle)
                return Put(context, (ProfileHandle)target);

            if (target is ProfilePropertyHandle)
                return Put(context, (ProfilePropertyHandle)target);

            if (target is ProfileCommandHandle)
                return Put(context, (ProfileCommandHandle)target);

            return Task.FromResult<object>(HttpStatusCode.MethodNotAllowed);
        }

        Task<object> Put(IContext context, ProfileHandle profile)
        {
            Contract.Requires<ArgumentNullException>(profile != null);

            return Task.FromResult<object>(HttpStatusCode.NotImplemented);
        }

        async Task<object> Put(IContext context, ProfilePropertyHandle property)
        {
            Contract.Requires<ArgumentNullException>(property != null);

            var obj = Bind<ProfilePropertyRequest>(context, new ProfilePropertyRequest() { Type = property.Get().GetType() });
            if (obj == null)
                return Task.FromResult<object>(HttpStatusCode.BadRequest);

            // set new property value
            property.Set(obj.Value);

            // return new property data
            return await PropertyToData(context, property);
        }

        Task<object> Put(IContext context, ProfileCommandHandle property)
        {
            Contract.Requires<ArgumentNullException>(property != null);

            var obj = Bind<ProfileCommandRequest>(context);
            if (obj == null)
                return Task.FromResult<object>(HttpStatusCode.BadRequest);

            // set new property value
            property.Invoke(obj.Parameters.Select(i => i.Value).ToArray());

            return Task.FromResult<object>(HttpStatusCode.OK);
        }

        /// <summary>
        /// Deserializes the data from the given request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        T Bind<T>(IContext context, T target)
        {
            foreach (var mediaRange in MediaRangeList.Parse(context.Request.ContentType) + "application/json")
            {
                foreach (var serializer in serializers)
                {
                    if (serializer.CanDeserialize(typeof(T), target, mediaRange))
                    {
                        return (T)serializer.Deserialize(typeof(T), target, context.Request.Body);
                    }
                }
            }

            return default(T);
        }

        T Bind<T>(IContext context)
        {
            return Bind<T>(context, default(T));
        }

    }

}
