using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

using Rnet.Drivers;
using Rnet.Profiles.Metadata;
using Rnet.Service.Processors;

namespace Rnet.Service.Objects
{

    [RequestProcessor(typeof(Profile))]
    public class ProfileRequestProcessor : RequestProcessor<Profile>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="module"></param>
        [ImportingConstructor]
        public ProfileRequestProcessor(
            ObjectModule module)
            : base(module)
        {

        }

        public override async Task<object> Resolve(Profile target, string[] path)
        {
            throw new NotImplementedException();
        }

        public override async Task<object> Get(Profile profile)
        {
            return new ProfileData()
            {
                Uri = await profile.GetUri(Context),
                Id = profile.Metadata.Id,
                Name = profile.Metadata.Name,
                Namespace = profile.Metadata.Namespace,
                Properties = await GetProperties(profile),
                Commands = await GetCommands(profile),
            };
        }

        async Task<ProfilePropertyDataCollection> GetProperties(Profile profile)
        {
            return new ProfilePropertyDataCollection((await Task.WhenAll(profile.Metadata.Properties
                .Select(async i => new
                {
                    Uri = await GetPropertyUri(profile, i),
                    Name = i.Name,
                    Value = i.GetValue(profile.Instance),
                })))
                .Select(i => new ProfilePropertyData()
                {
                    Uri = i.Uri,
                    Name = i.Name,
                    Value = i.Value,
                }));
        }

        async Task<Uri> GetPropertyUri(Profile profile, PropertyDescriptor property)
        {
            return (await profile.GetUri(Context)).UriCombine(property.Name);
        }

        async Task<ProfileCommandDataCollection> GetCommands(Profile profile)
        {
            return new ProfileCommandDataCollection((await Task.WhenAll(profile.Metadata.Operations
                .Select(async i => new
                {
                    Uri = await GetCommandUri(profile, i),
                    Name = i.Name,
                })))
                .Select(i => new ProfileCommandData()
                {
                    Uri = i.Uri,
                    Name = i.Name,
                }));
        }

        async Task<Uri> GetCommandUri(Profile profile, CommandDescriptor command)
        {
            return (await profile.GetUri(Context)).UriCombine(command.Name);
        }

    }

}
