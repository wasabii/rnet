using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rnet.Drivers;

namespace Rnet.Drivers.Profiles
{

    /// <summary>
    /// Serves as a basic <see cref="DriverPackage"/> that only includes controllers.
    /// </summary>
    public class DefaultControllerDriver : Driver
    {

        public sealed override Task<IEnumerable<Driver>> GetDriver(RnetBusObject target)
        {
            if (target is RnetController)
                return GetControllerProfiles((RnetController)target);
            else if (target is RnetZone)
                return GetZoneProfiles((RnetZone)target);
            else
                return Task.FromResult(Enumerable.Empty<Driver>());
        }

        /// <summary>
        /// Override this method to return applicable profiles for the target controller.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual Task<IEnumerable<Driver>> GetControllerProfiles(RnetController target)
        {
            return Task.FromResult(Enumerable.Empty<Driver>());
        }

        /// <summary>
        /// Override this method to return applicable profiles for the target zones.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual Task<IEnumerable<Driver>> GetZoneProfiles(RnetZone target)
        {
            return Task.FromResult(Enumerable.Empty<Driver>());
        }


    }

}
