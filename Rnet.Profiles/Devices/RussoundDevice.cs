using System.Collections.Generic;
using System.Threading.Tasks;
using Rnet.Profiles.Basic;

namespace Rnet.Profiles.Devices
{

    /// <summary>
    /// Basic Russound device profile. Provides functionality common to all Russound devices.
    /// </summary>
    [ProfileProvider]
    public class RussoundDevice : ProfileProvider
    {

        class ProfileImpl : IDevice
        {

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="device"></param>
            public ProfileImpl(RnetDevice device)
            {
                Info = new RussoundDeviceInfo(device);
            }

            /// <summary>
            /// Obtains device information.
            /// </summary>
            public DeviceInfo Info { get; private set; }

        }

        protected internal override IEnumerable<Task<IProfile>> GetProfilesAsync(RnetBusObject target)
        {
            yield return GetDeviceProfile(target);
        }

        /// <summary>
        /// Evaluates whether the <see cref="IDevice"/> implementation is compatible with the target.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        async Task<IProfile> GetDeviceProfile(RnetBusObject target)
        {
            var device = target as RnetDevice;
            if (device == null)
                return null;

            var d = await device.Directory.GetAsync(0, 0);
            if (d.Buffer != null &&
                d.Buffer[0] == 1)
                return new ProfileImpl(device);

            return null;
        }

    }

}
