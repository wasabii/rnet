using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace Rnet.Drivers
{

    /// <summary>
    /// Provides methods to query an RNET device for its supported driver.
    /// </summary>
    [Export(typeof(DriverManager))]
    public sealed class DriverManager
    {

        readonly IEnumerable<DriverPackage> packages;

        /// <summary>
        /// Initializes a new instances.
        /// </summary>
        /// <param name="packages"></param>
        [ImportingConstructor]
        public DriverManager(
            [ImportMany] IEnumerable<DriverPackage> packages)
        {
            Contract.Requires<ArgumentNullException>(packages != null);

            this.packages = packages.OrderBy(i => i);
        }

        /// <summary>
        /// Creates an instance of the appropriate driver by scanning registered driver packages. Returns <c>null</c>
        /// if no drivers are available for the specified <see cref="RnetDevice"/>.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        async Task<Driver> CreateDriver(RnetDevice device)
        {
            foreach (var package in packages)
            {
                var driver = await package.GetDriverInternal(device);
                if (driver != null)
                    return driver;
            }

            return null;
        }

        /// <summary>
        /// Obtains the <see cref="Driver"/> instance for the given device.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public Task<Driver> GetDriver(RnetDevice device)
        {
            Contract.Requires<ArgumentNullException>(device != null);

            return device.Context.GetOrCreate<Task<Driver>>(() => CreateDriver(device));
        }

    }

}
