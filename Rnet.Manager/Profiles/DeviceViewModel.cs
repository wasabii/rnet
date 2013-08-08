using Rnet.Drivers;
using Rnet.Profiles;

namespace Rnet.Manager.Profiles
{

    [ViewModel(typeof(IDevice))]
    public class DeviceViewModel : ViewModel<IDevice>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="profile"></param>
        public DeviceViewModel(Profile<IDevice> profile)
            : base(profile)
        {

        }

    }

}
