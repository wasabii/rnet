using System.Collections.Generic;

namespace Rnet.Profiles
{

    /// <summary>
    /// Provides extension methods for <see cref="RnetBusObject"/> instances to resolve profiles.
    /// </summary>
    public static class RnetBusObjectExtensions
    {

        /// <summary>
        /// Returns the set of profiles supported on the object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<RnetProfile> GetProfiles(this RnetBusObject obj)
        {
            yield break;
        }

        /// <summary>
        /// Returns the specified profile for interacting with the object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T GetProfile<T>(this RnetBusObject obj)
            where T : RnetProfile
        {
            return null;
        }

    }

}
