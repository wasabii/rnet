using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnet.Profiles
{

    /// <summary>
    /// Serves as a basic <see cref="ProfileProvider"/> that only includes controllers.
    /// </summary>
    public abstract class ControllerProfileProvider : ProfileProvider
    {

        public sealed override Task<IEnumerable<IProfile>> GetProfiles(RnetBusObject target)
        {
            if (target is RnetController)
                return GetControllerProfiles((RnetController)target);
            else if (target is RnetZone)
                return GetZoneProfiles((RnetZone)target);
            else
                return Task.FromResult(Enumerable.Empty<IProfile>());
        }

        /// <summary>
        /// Override this method to return applicable profiles for the target controller.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual Task<IEnumerable<IProfile>> GetControllerProfiles(RnetController target)
        {
            return Task.FromResult(Enumerable.Empty<IProfile>());
        }

        /// <summary>
        /// Override this method to return applicable profiles for the target zones.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual Task<IEnumerable<IProfile>> GetZoneProfiles(RnetZone target)
        {
            return Task.FromResult(Enumerable.Empty<IProfile>());
        }


    }

}
