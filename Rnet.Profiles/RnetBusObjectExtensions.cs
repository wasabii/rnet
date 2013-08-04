using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
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
        public static IObservable<KeyValuePair<Type, IProfile>> GetProfilesAsync(this RnetBusObject target)
        {
            return target.Extensions.GetOrCreate<IObservable<IProfile>>(() =>
                Providers.ToObservable()
                    .Select(i =>
                        Observable.FromAsync(() =>
                            i.GetProfilesAsync(target) ?? Task.FromResult(Enumerable.Empty<IProfile>())))
                    .Merge()
                    .Where(i => i != null)
                    .SelectMany(i => i)
                    .Where(i => i != null)
                    .Select(i =>
                        Observable.FromAsync(() =>
                            InitializeProfileAsync(i)))
                    .Merge())
                    .Select(i => new { Object = i, Profiles = GetProfileTypes(i) })
                    .SelectMany(i => i.Profiles.Select(j => new { Profile = j, Object = i.Object }))
                    .Select(i => new KeyValuePair<Type, IProfile>(i.Profile, i.Object));
        }

        /// <summary>
        /// Gets the supported profile implementation for the target of the given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Task<T> GetProfileAsync<T>(this RnetBusObject target)
            where T : class, IProfile
        {
            return target.Extensions.GetOrCreate<Task<T>>(async () =>
                await GetProfilesAsync(target)
                    .Where(i => i.Key == typeof(T))
                    .Select(i => i.Value)
                    .OfType<T>()
                    .FirstOrDefaultAsync());
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
