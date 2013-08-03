using System.Threading.Tasks;

namespace Rnet.Profiles
{

    /// <summary>
    /// Lifecycle methods for profile instances.
    /// </summary>
    public interface IProfileLifecycle
    {

        /// <summary>
        /// Invoked before the first usage. This could be used to conduct any required async initialization tasks.
        /// </summary>
        /// <returns></returns>
        Task InitializeAsync();

    }

}
