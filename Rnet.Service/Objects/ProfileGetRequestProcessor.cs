using System;
using System.Linq;
using System.Threading.Tasks;

using Nancy;

using Rnet.Drivers;
using Rnet.Profiles.Metadata;
using Rnet.Service.Processors;

namespace Rnet.Service.Objects
{

    [RequestProcessor]
    public class ProfileGetRequestProcessor : GenericRequestProcessor<Profile>
    {

        public override Task<bool> CanProcess(NancyContext context, string method, string[] uri, Profile target)
        {
            return Task.FromResult(method == "GET");
        }

        public override async Task<object> Process(NancyContext context, string method, string[] uri, Profile profile)
        {
            return new ProfileData()
            {
                Id = profile.Metadata.Id,
                Properties = await GetProperties(context, profile),
                Commands = await GetCommands(context, profile),
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
