using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnet.Profiles
{

    /// <summary>
    /// Serves as a basic <see cref="DriverPackage"/> that only includes devices.
    /// </summary>
    public abstract class DeviceProfileProvider : DriverPackage
    {

        public override sealed Task<IEnumerable<Driver>> GetDriver(RnetBusObject target)
        {
            return target is RnetDevice ? GetDeviceProfilesAsync((RnetDevice)target) : Task.FromResult(Enumerable.Empty<Driver>());
        }

        /// <summary>
        /// Override this method to return applicable profiles for the target device.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual Task<IEnumerable<Driver>> GetDeviceProfilesAsync(RnetDevice target)
        {
            return Task.FromResult(Enumerable.Empty<Driver>());
        }

    }

}
