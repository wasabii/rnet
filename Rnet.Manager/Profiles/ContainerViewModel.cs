using Rnet.Drivers;
using Rnet.Profiles;

namespace Rnet.Manager.Profiles
{

    [ViewModel(typeof(IContainer))]
    public class ContainerViewModel : ViewModel<IContainer>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="profile"></param>
        public ContainerViewModel(Profile<IContainer> profile)
            : base(profile)
        {

        }

    }

}
