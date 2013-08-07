using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Reactive.Linq;
using Nito.AsyncEx;

namespace Rnet.Drivers
{

    /// <summary>
    /// Provides methods to query an RNET device for its supported driver.
    /// </summary>
    public static class DriverManager
    {

        static readonly AsyncLock lck = new AsyncLock();
        static readonly List<DriverPackage> packages = new List<DriverPackage>();

        /// <summary>
        /// Registers a driver package instance.
        /// </summary>
        /// <param name="package"></param>
        public static void Register(this DriverPackage package)
        {
            // insert package at the beginning so we scan it ahead of built-in drivers
            lock (lck)
                if (!packages.Contains(package))
                    packages.Insert(0, package);
        }

        /// <summary>
        /// Creates an instance of the appropriate driver by scanning registered driver packages. Returns <c>null</c>
        /// if no drivers are available for the specified <see cref="RnetDevice"/>.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        static async Task<Driver> CreateDriver(RnetDevice device)
        {
            return (await Task.WhenAll(packages
                .Select(i => i.GetDriver(device))))
                .OrderByDescending(i => i.Priority)
                .FirstOrDefault();
        }

        /// <summary>
        /// Obtains the <see cref="Driver"/> instance for the given device.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public static Task<Driver> GetDriver(this RnetDevice device)
        {
            return device.Context.GetOrCreate<Task<Driver>>(() => CreateDriver(device));
        }

    }

}
