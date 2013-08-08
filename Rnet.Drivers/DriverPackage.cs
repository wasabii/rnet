using System;
using System.Threading.Tasks;

namespace Rnet.Drivers
{

    /// <summary>
    /// Provides the ability to resolve a <see cref="Driver"/> instance for <see cref="RnetDevice"/> objects.
    /// </summary>
    public abstract class DriverPackage : IComparable<DriverPackage>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected DriverPackage()
        {

        }

        /// <summary>
        /// Relative priority of all drivers in the package.
        /// </summary>
        public virtual DriverPriority Priority
        {
            get { return DriverPriority.Unconfigured; }
        }

        /// <summary>
        /// Returns a driver for the specified RNET device or <c>null</c> if the package does not contain one.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        protected internal abstract Task<Driver> GetDriver(RnetDevice device);

        int IComparable<DriverPackage>.CompareTo(DriverPackage other)
        {
            return Priority.CompareTo(other.Priority);
        }

    }

}
