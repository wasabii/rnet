using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

using Nancy;

using Rnet.Drivers;
using Rnet.Profiles.Metadata;
using Rnet.Service.Processors;

namespace Rnet.Service.Objects
{

    [RequestProcessor(typeof(Profile))]
    public class ProfileRequestProcessor : RequestProcessor<Profile>
    {

        public ProfileRequestProcessor(
            ObjectModule module)
            : base(module)
        {

        }

        public Profile Profile
        {
            get { return (Profile)base.Object; }
        }

        public override async Task<object> Get()
        {
            return new ProfileData()
            {
                Id = Profile.Metadata.Id,
                Properties = await GetProperties(Context, Profile),
                Commands = await GetCommands(Context, Profile),
            };
        }

        async Task<ProfilePropertyDataCollection> GetProperties(NancyContext context, Profile profile)
        {
            return new ProfilePropertyDataCollection((await Task.WhenAll(profile.Metadata.Properties
                .Select(async i => new
                {
                    Href = await GetPropertyUri(context, profile, i),
                    Name = i.Name,
                    Value = i.GetValue(profile.Instance),
                })))
                .Select(i => new ProfilePropertyData()
                {
                    Href = i.Href,
                    Name = i.Name,
                    Value = i.Value,
                }));
        }

        async Task<Uri> GetPropertyUri(NancyContext context, Profile profile, PropertyDescriptor property)
        {
            return (await profile.GetProfileUri(context)).UriCombine(property.Name).MakeRelativeUri(context);
        }

        async Task<ProfileCommandDataCollection> GetCommands(NancyContext context, Profile profile)
        {
            return new ProfileCommandDataCollection((await Task.WhenAll(profile.Metadata.Operations
                .Select(async i => new
                {
                    Href = await GetCommandUri(context, profile, i),
                    Name = i.Name,
                })))
                .Select(i => new ProfileCommandData()
                {
                    Href = i.Href,
                    Name = i.Name,
                }));
        }

        async Task<Uri> GetCommandUri(NancyContext context, Profile profile, CommandDescriptor command)
        {
            return (await profile.GetProfileUri(context)).UriCombine(command.Name).MakeRelativeUri(context);
        }

    }

}
