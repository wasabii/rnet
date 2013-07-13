using Rnet.Protocol;

namespace Rnet.Model
{

    /// <summary>
    /// Base class of RNET devices.
    /// </summary>
    public abstract class Device
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected Device()
        {
            Items = new DataItemCollection();
        }

        /// <summary>
        /// Gets the ID of the device on the RNET bus.
        /// </summary>
        public abstract RnetDeviceId Id { get; }

        /// <summary>
        /// Gets the set of items stored in this device.
        /// </summary>
        public DataItemCollection Items { get; private set; }

    }

}
