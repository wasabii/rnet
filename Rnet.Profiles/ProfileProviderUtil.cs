using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Rnet.Profiles
{

    /// <summary>
    /// Provides extension methods for <see cref="RnetBusObject"/> instances to resolve profiles.
    /// </summary>
    public static class ProfileProviderUtil
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
        /// Initializes the given profile and returns it.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        static async Task<IProfile> InitializeProfileAsync(IProfile profile)
        {
            var l = profile as IProfileLifecycle;
            if (l != null)
                await l.InitializeAsync();

            return profile;
        }

        /// <summary>
        /// Gets the supported profile types.
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
        /// Gets the set of supported profiles for the target.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Task<Dictionary<Type, IProfile>> GetProfiles(this RnetBusObject target)
        {
            return target.Context.GetOrCreate<Task<Dictionary<Type, IProfile>>>(async () =>
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
                .ToDictionary(i => i.Key, i => i.First().Object));
        }

        /// <summary>
        /// Gets the supported profile implementation for the target of the given type.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="profileType"></param>
        /// <returns></returns>
        public static Task<IProfile> GetProfile(this RnetBusObject target, Type profileType)
        {
            // key stored in context is a generic task type
            var t = typeof(Task<>)
                .MakeGenericType(profileType);

            // value stored in context is a func which returns an implementation
            var f = (Func<Task<IProfile>>)(async () =>
                await GetProfiles(target)
                    .Where(i => i.Key == profileType)
                    .Select(i => i.Value)
                    .OfType<IProfile>()
                    .FirstOrDefaultAsync());

            // obtain it or add a new one
            return (Task<IProfile>)target.Context.GetOrCreate(t, f);
        }

        /// <summary>
        /// Gets the supported profile implementation for the target of the given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<T> GetProfile<T>(this RnetBusObject target)
            where T : class, IProfile
        {
            return (T)await GetProfile(target, typeof(T));
        }

    }

}
