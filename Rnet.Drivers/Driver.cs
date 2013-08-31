using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rnet.Drivers
{

    /// <summary>
    /// Base class for a driver. A driver instance associates with a single <see cref="RnetDevice"/> and provides a
    /// set of profiles for interacting with the device in a standardized fashion. User applications should not access
    /// the <see cref="Driver"/> class directly, but rather resolve its profiles through the <see 
    /// cref="ProfileManager"/>.
    /// </summary>
    public abstract class Driver : IComparable<Driver>
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
        /// Driver implementations should implement this method to return the set of profile implementation instances
        /// supported by the underlying device.
        /// </summary>
        protected abstract Task<object[]> GetProfiles();

        /// <summary>
        /// Internal exposure of GetProfiles.
        /// </summary>
        /// <returns></returns>
        internal Task<object[]> GetProfilesInternal()
        {
            return GetProfiles();
        }

        public int CompareTo(Driver other)
        {
            return Priority.CompareTo(other.Priority);
        }

    }

}
