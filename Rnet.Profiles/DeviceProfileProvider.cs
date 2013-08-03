using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rnet.Profiles
{

    /// <summary>
    /// Serves as a basic <see cref="ProfileProvider"/> that only includes devices.
    /// </summary>
    public abstract class DeviceProfileProvider : ProfileProvider
    {

        public override sealed Task<IEnumerable<IProfile>> GetProfilesAsync(RnetBusObject target)
        {
            return target is RnetDevice ? GetDeviceProfilesAsync((RnetDevice)target) : null;
        }

        /// <summary>
        /// Override this method to return applicable profiles for the target device.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public abstract Task<IEnumerable<IProfile>> GetDeviceProfilesAsync(RnetDevice target);

    }

}
