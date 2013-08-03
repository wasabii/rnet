using System.Threading;
using System.Threading.Tasks;

namespace Rnet.Profiles.Basic
{

    /// <summary>
    /// Base class for device information implementations.
    /// </summary>
    public abstract class DeviceInfo : ModelObject
    {

        /// <summary>
        /// Gets the device model.
        /// </summary>
        public abstract string Model { get; }

        /// <summary>
        /// Gets the device manufacturer.
        /// </summary>
        public abstract string Manufacturer { get; }

        /// <summary>
        /// Gets the device firmware.
        /// </summary>
        public abstract string FirmwareVersion { get; }

        /// <summary>
        /// Waits for the data to be available.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task WaitAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Refreshes the data.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task RefreshAsync(CancellationToken cancellationToken);

    }

}
