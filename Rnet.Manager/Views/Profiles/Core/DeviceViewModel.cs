using Rnet.Drivers;
using Rnet.Profiles.Core;

namespace Rnet.Manager.Profiles.Core
{

    [ProfileViewModel(typeof(IDevice))]
    public class DeviceViewModel : ProfileViewModel<IDevice>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="profile"></param>
        public DeviceViewModel(ProfileHandle<IDevice> profile)
            : base(profile)
        {

        }

    }

}
