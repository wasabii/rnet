using System.Threading.Tasks;

using Rnet.Profiles;

namespace Rnet.Drivers
{

    /// <summary>
    /// Provides an optional base implementation of a profile.
    /// </summary>
    public abstract class ProfileBase : ModelObject, IProfileLifecycle
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        protected ProfileBase(RnetBusObject target)
        {
            Target = target;
        }

        /// <summary>
        /// The object for which this profile is associated.
        /// </summary>
        protected RnetBusObject Target { get; private set; }

        /// <summary>
        /// Initializes the profile state.
        /// </summary>
        /// <returns></returns>
        protected virtual Task Initialize()
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// Implements IProfileLifecycle.Initialize.
        /// </summary>
        /// <returns></returns>
        Task IProfileLifecycle.Initialize()
        {
            return Initialize();
        }

    }

}
