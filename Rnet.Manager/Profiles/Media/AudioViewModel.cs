using Rnet.Drivers;
using Rnet.Profiles.Media;

namespace Rnet.Manager.Profiles.Media
{

    [ViewModel(typeof(IAudio))]
    public class AudioViewModel : ViewModel<IAudio>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="profile"></param>
        public AudioViewModel(Profile<IAudio> profile)
            : base(profile)
        {

        }

    }

}
