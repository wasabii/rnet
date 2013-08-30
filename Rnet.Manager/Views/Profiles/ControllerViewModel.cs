using Rnet.Drivers;
using Rnet.Profiles;

namespace Rnet.Manager.Profiles
{

    [ViewModel(typeof(IController))]
    public class ControllerViewModel : ViewModel<IController>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="profile"></param>
        public ControllerViewModel(Profile<IController> profile)
            : base(profile)
        {

        }

    }

}
