using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rnet.Profiles
{

    /// <summary>
    /// Provides extension methods for <see cref="RnetBusObject"/> instances to resolve profiles.
    /// </summary>
    public static class RnetBusObjectExtensions
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
        /// Gets the set of supported profiles for the target.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static IEnumerable<Task<IProfile>> GetProfilesAsync(this RnetBusObject target)
        {
            return Providers.SelectMany(i => i.GetProfilesAsync(target));
        }

        /// <summary>
        /// Gets the set of supported profiles for the target.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static IEnumerable<IProfile> GetProfiles(this RnetBusObject target)
        {
            foreach (var t in GetProfilesAsync(target))
                if (t.Result is IProfile)
                    yield return (IProfile)t.Result;
        }

        /// <summary>
        /// Gets the supported profile implementation for the target of the given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<T> GetProfileAsync<T>(this RnetBusObject target)
            where T : class, IProfile
        {
            foreach (var t in GetProfilesAsync(target))
                if ((await t) is T)
                    return (T)await t;

            return null;
        }

        /// <summary>
        /// Gets the supported profile implementation for the target of the given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        public static T GetProfile<T>(this RnetBusObject target)
            where T : class, IProfile
        {
            return GetProfileAsync<T>(target).Result;
        }

    }

}
