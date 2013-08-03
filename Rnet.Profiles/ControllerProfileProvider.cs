using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rnet.Profiles
{

    /// <summary>
    /// Serves as a basic <see cref="ProfileProvider"/> that only includes controllers.
    /// </summary>
    public abstract class ControllerProfileProvider : DeviceProfileProvider
    {

        public override sealed Task<IEnumerable<IProfile>> GetDeviceProfilesAsync(RnetDevice target)
        {
            return target is RnetController ? GetControllerProfilesAsync((RnetController)target) : null;
        }

        /// <summary>
        /// Override this method to return applicable profiles for the target controller.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public abstract Task<IEnumerable<IProfile>> GetControllerProfilesAsync(RnetController target);

    }

}
