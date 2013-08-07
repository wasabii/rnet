using System.Threading.Tasks;

namespace Rnet.Profiles
{

    /// <summary>
    /// Lifecycle methods for profile instances. Profiles need not implement this interface if they do not want the
    /// desired functionality.
    /// </summary>
    public interface IProfileLifecycle
    {

        /// <summary>
        /// Invoked before the first usage of the profile. Provides an asynchronous location to conduct any
        /// initialization of device state.
        /// </summary>
        /// <returns></returns>
        Task Initialize();

    }

}
