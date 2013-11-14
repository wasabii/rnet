using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace Rnet.Drivers
{

    /// <summary>
    /// Provides the ability to resolve a driver for a device.
    /// </summary>
    [InheritedExport(typeof(DriverPackage))]
    public abstract class DriverPackage :
        IComparable<DriverPackage>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        [ImportingConstructor]
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
        internal Task<Driver> GetDriverInternal(RnetDevice device)
        {
            return GetDriver(device);
        }

        /// <summary>
        /// Returns a driver for the specified RNET device or <c>null</c> if the package does not contain one.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        protected abstract Task<Driver> GetDriver(RnetDevice device);

        /// <summary>
        /// Compares this instance to the specified <see cref="DriverPackage"/>.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        int IComparable<DriverPackage>.CompareTo(DriverPackage other)
        {
            return Priority.CompareTo(other.Priority);
        }

    }

}
