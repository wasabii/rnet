using System;
using System.Threading.Tasks;

using Rnet.Drivers.Profiles;

namespace Rnet.Drivers
{

    /// <summary>
    /// Provides methods to query an RNET bus object for its supported profiles.
    /// </summary>
    public static class ProfileManager
    {

        /// <summary>
        /// Creates an instance of the appropriate profile by asking the driver established for the device.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="profileType"></param>
        /// <returns></returns>
        static async Task<object> CreateProfile(Driver target, Type profileType)
        {
            // obtain profile
            var profile = await target.GetProfile(profileType);
            if (profile == null)
                return null;

            if (!profileType.IsInstanceOfType(profile))
                throw new RnetException("Driver returned profile instance that did not implement requested profile type.");

            return profile;
        }

        /// <summary>
        /// Creates an instance of the requested profile type.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        static async Task<object> CreateProfile(RnetDevice target, Type profileType)
        {
            if (!profileType.IsInterface)
                throw new RnetException("A profile type must be an interface.");

            // obtain driver
            var driver = await target.GetDriver();
            if (driver == null)
                return null;

            // create profile
            return await CreateProfile(driver, profileType);
        }

        /// <summary>
        /// Creates an instance of the requested profile type for the specified <see cref="RnetZone"/>. The profile is
        /// obtained from the <see cref="IBusObjectOwner"/> implementation provided by the <see cref="RnetController"/>
        /// that is responsible for the zone. If the <see cref="RnetController"/> does not implement <see
        /// cref="IBusObjectContainer"/> then no profile will be found and <c>null</c> will be returned.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="profileType"></param>
        /// <returns></returns>
        static async Task<object> CreateProfile(RnetZone target, Type profileType)
        {
            var owner = await target.Controller.GetProfile<IBusObjectOwner>();
            if (owner != null)
                return await owner.GetProfile(target, profileType,
                    target.Context.Get<ContainerContext>());

            return null;
        }

        /// <summary>
        /// Creates an instance of the requested profile type for the specified <see cref="LogicalBusObject"/>.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="profileType"></param>
        /// <returns></returns>
        static Task<object> CreateProfile(LogicalBusObject target, Type profileType)
        {
            var owner = target.Context.Get<ContainerContext>().Owner;
            if (owner != null)
                return owner.GetProfile(target, profileType,
                    target.Context.Get<ContainerContext>());

            return null;
        }

        /// <summary>
        /// Creates an instance of the requested profile type for the specified <see cref="RnetBusObject"/>.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="profileType"></param>
        /// <returns></returns>
        static Task<object> CreateProfile(RnetBusObject target, Type profileType)
        {
            if (target is RnetDevice)
                return CreateProfile((RnetDevice)target, profileType);
            if (target is RnetZone)
                return CreateProfile((RnetZone)target, profileType);
            if (target is LogicalBusObject)
                return CreateProfile((LogicalBusObject)target, profileType);

            return null;
        }

        /// <summary>
        /// Obtains the profile implementation of the requested type for the given <see cref="RnetBusObject"/>.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="profileType"></param>
        /// <returns></returns>
        public static Task<object> GetProfile(this RnetBusObject target, Type profileType)
        {
            return (Task<object>)target.Context.GetOrCreate(
                typeof(Task<>).MakeGenericType(profileType), () =>
                    CreateProfile(target, profileType));
        }

        /// <summary>
        /// Obtains the profile implementation of the requested type for the given <see cref="RnetBusObject"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        public static async Task<T> GetProfile<T>(this RnetBusObject target)
            where T : class
        {
            return (T)await GetProfile(target, typeof(T));
        }

    }

}
