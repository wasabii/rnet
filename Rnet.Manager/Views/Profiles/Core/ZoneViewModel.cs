using Rnet.Drivers;
using Rnet.Profiles.Core;

namespace Rnet.Manager.Profiles.Core
{

    [ProfileViewModel(typeof(IZone))]
    public class ZoneViewModel : ProfileViewModel<IZone>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="profile"></param>
        public ZoneViewModel(ProfileHandle<IZone> profile)
            : base(profile)
        {

        }

    }

}
