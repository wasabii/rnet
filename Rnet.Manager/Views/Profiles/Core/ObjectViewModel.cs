using Rnet.Drivers;
using Rnet.Profiles.Core;

namespace Rnet.Manager.Profiles.Core
{

    [ProfileViewModel(typeof(IObject))]
    public class ObjectViewModel : ProfileViewModel<IObject>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="profile"></param>
        public ObjectViewModel(ProfileHandle<IObject> profile)
            : base(profile)
        {

        }

    }

}
