using Rnet.Drivers;
using Rnet.Profiles;

namespace Rnet.Manager.Profiles
{

    [ViewModel(typeof(IObject))]
    public class ObjectViewModel : ViewModel<IObject>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="profile"></param>
        public ObjectViewModel(Profile<IObject> profile)
            : base(profile)
        {

        }

    }

}
