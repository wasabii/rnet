using System;
using System.Threading.Tasks;

namespace Rnet.Drivers
{

    /// <summary>
    /// Base class for a driver. A driver instance associates with a single <see cref="RnetDevice"/> and provides a
    /// set of profiles for interacting with the device in a standardized fashion. User applications should not access
    /// the <see cref="Driver"/> class directly, but rather resolve its profiles through the <see 
    /// cref="ProfileManager"/>.
    /// </summary>
    public abstract class Driver
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="device"></param>
        protected internal Driver(RnetDevice device)
        {
            Device = device;
        }

        /// <summary>
        /// Returns the <see cref="RnetDevice"/> associated with this driver.
        /// </summary>
        public RnetDevice Device { get; private set; }

        /// <summary>
        /// Gets the relative driver priority.
        /// </summary>
        public virtual DriverPriority Priority
        {
            // implementation must override to take priority over native drivers
            get { return DriverPriority.Unconfigured; }
        }

        /// <summary>
        /// Returns a class implementing the requested profile type if the driver implements support for that type. If
        /// support is not available <c>null</c> should be returned. Implementations of this method should probe the
        /// device in order to determine if they offer support for it.
        /// </summary>
        /// <param name="profileType"></param>
        /// <returns></returns>
        protected internal Task<object> GetProfile(Type profileType)
        {
            return Task.FromResult<object>(null);
        }

    }

}
