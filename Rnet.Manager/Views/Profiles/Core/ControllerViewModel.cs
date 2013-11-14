using Rnet.Drivers;
using Rnet.Profiles.Core;

namespace Rnet.Manager.Profiles.Core
{

    [ProfileViewModel(typeof(IController))]
    public class ControllerViewModel : ProfileViewModel<IController>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="profile"></param>
        public ControllerViewModel(ProfileHandle<IController> profile)
            : base(profile)
        {

        }

    }

}
