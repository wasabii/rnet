using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Rnet.Profiles;
using Rnet.Profiles.Core;
using Rnet.Profiles.Metadata;

namespace Rnet.Drivers
{

    /// <summary>
    /// Provides methods to query an RNET bus object for its supported profiles.
    /// </summary>
    [Export(typeof(ProfileManager))]
    public sealed class ProfileManager
    {

        /// <summary>
        /// Caches the set of loaded profiles for each bus object.
        /// </summary>
        class Cache
        {

            readonly DriverManager driverManager;
            readonly ProfileManager profileManager;
            readonly RnetBusObject target;
            readonly Lazy<Task<ProfileHandle[]>> profiles;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="target"></param>
            public Cache(
                DriverManager driverManager,
                ProfileManager profileManager,
                RnetBusObject target)
            {
                Contract.Requires<ArgumentNullException>(driverManager != null);
                Contract.Requires<ArgumentNullException>(profileManager != null);
                Contract.Requires<ArgumentNullException>(target != null);

                this.driverManager = driverManager;
                this.profileManager = profileManager;
                this.target = target;

                this.profiles = new Lazy<Task<ProfileHandle[]>>(async () => await CreateProfiles(RequestProfiles()), true);
            }

            /// <summary>
            /// Obtains the set of profiles supported by the specified device.
            /// </summary>
            /// <param name="device"></param>
            /// <returns></returns>
            async Task<object[]> RequestProfiles(RnetDevice device)
            {
                Contract.Requires<ArgumentNullException>(device != null);

                var driver = await driverManager.GetDriver(device);
                if (driver != null)
                    return await driver.GetProfilesInternal();

                return null;
            }

            /// <summary>
            /// Obtains the set of profiles supported by the specified zone.
            /// </summary>
            /// <param name="zone"></param>
            /// <returns></returns>
            async Task<object[]> RequestProfiles(RnetZone zone)
            {
                Contract.Requires<ArgumentNullException>(zone != null);

                var owner = await profileManager.GetProfile<IOwner>(zone.Controller);
                if (owner != null)
                    return await owner.GetProfiles(zone);

                return null;
            }

            /// <summary>
            /// Obtains the set of profiles supported by the bus object.
            /// </summary>
            /// <returns></returns>
            async Task<object[]> RequestProfiles()
            {
                Contract.Assert(target != null);

                if (target is RnetDevice)
                    return await RequestProfiles((RnetDevice)target);

                var context = target.Context.Get<IContainerContext>();
                if (context != null &&
                    context.Owner != null)
                {
                    var profile = await profileManager.GetProfile<IOwner>(context.Owner);
                    if (profile != null)
                        return await profile.GetProfiles(target);
                }

                return null;
            }

            /// <summary>
            /// Creates a generic <see cref="ProfileHandle"/> instance that wraps the given information.
            /// </summary>
            /// <param name="target"></param>
            /// <param name="contract"></param>
            /// <param name="instance"></param>
            /// <returns></returns>
            ProfileHandle CreateProfile(RnetBusObject target, ProfileDescriptor contract, object instance)
            {
                Contract.Requires<ArgumentNullException>(target != null);
                Contract.Requires<ArgumentNullException>(contract != null);
                Contract.Requires<ArgumentNullException>(instance != null);

                return (ProfileHandle)Activator.CreateInstance(
                    typeof(ProfileHandle<>).MakeGenericType(contract.Contract),
                        target, contract, instance);
            }

            /// <summary>
            /// Extracts the supported profile types out of the given instance and returns a set of <see
            /// cref="ProfileHandle"/> objects.
            /// </summary>
            /// <param name="instance"></param>
            /// <returns></returns>
            IEnumerable<ProfileHandle> CreateProfiles(object instance)
            {
                Contract.Requires<ArgumentNullException>(instance != null);

                return instance.GetType().GetInterfaces()
                    .Select(i => profileManager.GetOrCreateMetadata(i))
                    .Where(i => i != null)
                    .Select(i => CreateProfile(target, i, instance));
            }

            /// <summary>
            /// Accepts a list of profile instances and emits a set of completed <see cref="ProfileHandle"/> classes which
            /// contain the metadata.
            /// </summary>
            /// <param name="instances"></param>
            /// <returns></returns>
            async Task<ProfileHandle[]> CreateProfiles(Task<object[]> instances)
            {
                Contract.Requires<ArgumentNullException>(instances != null);

                var o = await instances;
                if (o == null)
                    return new ProfileHandle[0];

                var l = o
                    .SelectMany(i => CreateProfiles(i))
                    .GroupBy(i => i.Metadata)
                    .Select(i => i.First())
                    .ToArray();

                // initialize any profiles that require it
                foreach (var p in l.Select(i => i.Instance).OfType<IProfileLifecycle>().Distinct())
                    await p.Initialize();

                return l;
            }

            /// <summary>
            /// Gets the set of profiles supported by the bus object.
            /// </summary>
            /// <returns></returns>
            public Task<ProfileHandle[]> GetProfiles()
            {
                // fetch from catch or build
                return profiles.Value;
            }

        }

        readonly DriverManager driverManager;
        readonly ConcurrentDictionary<Type, ProfileDescriptor> cache;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="driverManager"></param>
        [ImportingConstructor]
        public ProfileManager(
            DriverManager driverManager)
        {
            Contract.Requires<ArgumentNullException>(driverManager != null);

            this.driverManager = driverManager;
            this.cache = new ConcurrentDictionary<Type, ProfileDescriptor>();
        }

        /// <summary>
        /// Creates a new metadata instance for the given contract type.
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        ProfileDescriptor CreateMetadata(Type contract)
        {
            Contract.Requires<ArgumentNullException>(contract != null);

            var attr = contract.GetCustomAttribute<ProfileContractAttribute>();
            if (attr == null)
                return null;

            var d = new ProfileDescriptor();
            d.Load(contract);
            return d;
        }

        /// <summary>
        /// Either retrieves or creates metadata for the given info.
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        ProfileDescriptor GetOrCreateMetadata(Type contract)
        {
            Contract.Requires<ArgumentNullException>(contract != null);

            return cache.GetOrAdd(contract, m =>
                CreateMetadata(contract));
        }

        /// <summary>
        /// Obtains the set of supported profiles for the given bus object.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public Task<ProfileHandle[]> GetProfiles(RnetBusObject target)
        {
            Contract.Requires<ArgumentNullException>(target != null);

            return target.Context.GetOrCreate<Cache>(() =>
                new Cache(driverManager, this, target))
                    .GetProfiles();
        }

        /// <summary>
        /// Obtains the profile implementation of the requested type for the given <see cref="RnetBusObject"/>.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="contract"></param>
        /// <returns></returns>
        public async Task<object> GetProfile(RnetBusObject target, Type contract)
        {
            Contract.Requires<ArgumentNullException>(target != null);
            Contract.Requires<ArgumentNullException>(contract != null);

            return (await GetProfiles(target))
                .Where(i => i.Metadata.Contract == contract)
                .Select(i => i.Instance);
        }

        /// <summary>
        /// Obtains the profile implementation of the requested type for the given <see cref="RnetBusObject"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        public async Task<T> GetProfile<T>(RnetBusObject target)
            where T : class
        {
            Contract.Requires<ArgumentNullException>(target != null);

            return (await GetProfiles(target))
                .Where(i => i.Metadata.Contract == typeof(T))
                .Select(i => i.Instance)
                .OfType<T>()
                .FirstOrDefault();
        }

    }

}
