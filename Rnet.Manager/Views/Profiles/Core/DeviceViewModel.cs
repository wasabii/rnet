using Rnet.Drivers;
using Rnet.Profiles.Core;

namespace Rnet.Manager.Profiles.Core
{

    [ViewModel(typeof(IDevice))]
    public class DeviceViewModel : ViewModel<IDevice>
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
