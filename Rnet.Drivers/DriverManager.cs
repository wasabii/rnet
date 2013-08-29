using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Nito.AsyncEx;

namespace Rnet.Drivers
{

    /// <summary>
    /// Provides methods to query an RNET device for its supported driver.
    /// </summary>
    public static class DriverManager
    {

        static readonly AsyncLock lck = new AsyncLock();
        static readonly SortedSet<DriverPackage> packages = new SortedSet<DriverPackage>();

        /// <summary>
        /// Initializes the default instance.
        /// </summary>
        static DriverManager()
        {
            // ensure default drivers are always available
            Register<Default.DriverPackage>();
        }

        /// <summary>
        /// Registers a driver package by type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Register<T>()
            where T : DriverPackage, new()
        {
            lock (lck)
                if (!packages.Any(i => i.GetType() == typeof(T)))
                    if (!packages.Add(new T()))
                        throw new Exception("Unknown error adding package.");
        }

        /// <summary>
        /// Creates an instance of the appropriate driver by scanning registered driver packages. Returns <c>null</c>
        /// if no drivers are available for the specified <see cref="RnetDevice"/>.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        static async Task<Driver> CreateDriver(RnetDevice device)
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
        public static Task<Driver> GetDriver(this RnetDevice device)
        {
            return device.Context.GetOrCreate<Task<Driver>>(() => CreateDriver(device));
        }

    }

}
