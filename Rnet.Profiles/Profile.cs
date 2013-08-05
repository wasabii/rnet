using System.Threading.Tasks;

namespace Rnet.Profiles
{

    /// <summary>
    /// Serves as a simple profile implementation base for a device.
    /// </summary>
    public abstract class Profile : ModelObject, IProfile, IProfileLifecycle
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="device"></param>
        protected internal Profile(RnetBusObject target)
        {
            Target = target;
        }

        /// <summary>
        /// The controller implementing this profile.
        /// </summary>
        protected RnetBusObject Target { get; private set; }

        /// <summary>
        /// Invoked before the first usage. This could be used to conduct any required async initialization tasks.
        /// </summary>
        /// <returns></returns>
        protected virtual Task InitializeAsync()
        {
            return Task.FromResult(false);
        }

        Task IProfileLifecycle.InitializeAsync()
        {
            return InitializeAsync();
        }

    }

}
