using Rnet.Drivers;
using Rnet.Profiles;

namespace Rnet.Manager.Profiles
{

    [ViewModel(typeof(IZone))]
    public class ZoneViewModel : ViewModel<IZone>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="profile"></param>
        public ZoneViewModel(Profile<IZone> profile)
            : base(profile)
        {

        }

    }

}
