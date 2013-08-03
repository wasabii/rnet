using Rnet.Profiles.Capabilities;

namespace Rnet.Profiles
{

    /// <summary>
    /// Base device profile class. A <see cref="Device"/> instance should be associated with each device based on it's make
    /// and model. A <see cref="Device"/> additionally provides various capabilities for interacting with the device.
    /// </summary>
    public abstract class Device : IDevice
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="device"></param>
        protected Device(RnetDevice device)
        {
            Device = device;
        }

        /// <summary>
        /// Reference to device on BUS.
        /// </summary>
        public RnetDevice Device { get; private set; }

        /// <summary>
        /// Gets the device info structure.
        /// </summary>
        public abstract DeviceInfo Info { get; }

    }

}
