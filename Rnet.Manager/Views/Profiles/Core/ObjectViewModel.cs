using Rnet.Drivers;
using Rnet.Profiles.Core;

namespace Rnet.Manager.Profiles.Core
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
