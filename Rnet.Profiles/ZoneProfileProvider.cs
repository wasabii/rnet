using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Rnet.Profiles.Basic;

namespace Rnet.Profiles
{

    /// <summary>
    /// Serves as a basic <see cref="ProfileProvider"/> that only includes zones.
    /// </summary>
    public abstract class ZoneProfileProvider : ProfileProvider
    {

        public override sealed Task<IEnumerable<IProfile>> GetProfilesAsync(RnetBusObject target)
        {
            return target is RnetZone ? GetControllerZoneProfilesAsync((RnetZone)target) : null;
        }

        /// <summary>
        /// Override this method to return applicable profiles for the target zone.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        async Task<IEnumerable<IProfile>> GetControllerZoneProfilesAsync(RnetZone zone)
        {
            // controller will provide profiles if possible
            var c = await zone.Controller.GetProfileAsync<IController>();

            // allow the controller to decide what profiles I support
            var e1 = c != null ? c.GetZoneProfilesAsync(zone) : Task.FromResult(Enumerable.Empty<IProfile>());
            var e2 = GetZoneProfilesAsync(zone);
            return Enumerable.Concat(await e1, await e2);
        }

        /// <summary>
        /// Allows a custom zone implementation to add additional profiles.
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public virtual Task<IEnumerable<IProfile>> GetZoneProfilesAsync(RnetZone zone)
        {
            return Task.FromResult(Enumerable.Empty<IProfile>());
        }

    }

}
