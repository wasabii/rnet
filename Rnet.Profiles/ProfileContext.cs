using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rnet.Profiles
{

    /// <summary>
    /// Provides access to the profiles available for an <see cref="RnetBusObject"/>.
    /// </summary>
    public class ProfileContext
    {

        /// <summary>
        /// All available providers.
        /// </summary>
        static readonly IEnumerable<ProfileProvider> Providers = typeof(ProfileProvider).Assembly.GetTypes()
            .Select(i => new { Type = i, Attribute = i.GetCustomAttribute<ProfileProviderAttribute>() })
            .Where(i => i.Attribute != null)
            .Select(i => i.Type)
            .Select(i => (ProfileProvider)Activator.CreateInstance(i))
            .ToList();

        /// <summary>
        /// Returns the unique set of supported <see cref="IProfile"/> interface types supported by the given
        /// implementation instance.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        static IEnumerable<Type> GetProfileTypes(IProfile profile)
        {
            return profile.GetType().GetInterfaces()
                .Where(i => typeof(IProfile).IsAssignableFrom(i))
                .Where(i => i != typeof(IProfile))
                .Distinct();
        }

        /// <summary>
        /// Returns a dictionary of the supported profile types and their implementation instances for the given
        /// target object.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        async Task<Dictionary<Type, IProfile>> QueryProfileProviders(RnetBusObject target)
        {
            return 
                (await Task.WhenAll(
                    Providers
                        .Select(async i =>
                            (await i.GetProfiles(target) ?? Enumerable.Empty<IProfile>())
                                .ToList())))
                .SelectMany(i => i)
                .Where(i => i != null)
                .Select(i => new { Object = i, ProfileTypes = GetProfileTypes(i) })
                .SelectMany(i => i.ProfileTypes.Select(j => new { ProfileType = j, Object = i.Object }))
                .GroupBy(i => i.ProfileType)
                .ToDictionary(i => i.Key, i => i.First().Object);
        }

        RnetBusObject
        Task<IDictionary<Type, IProfile>> profiles;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        ProfileContext(RnetBusObject target)
        {
            
        }

    }

}
