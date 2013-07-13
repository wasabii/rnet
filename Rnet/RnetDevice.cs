namespace Rnet
{

    /// <summary>
    /// Base class of RNET devices.
    /// </summary>
    public abstract class RnetDevice
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected RnetDevice()
        {
            Items = new RnetDataItemCollection();
        }

        /// <summary>
        /// Gets the ID of the device on the RNET bus.
        /// </summary>
        public abstract RnetDeviceId Id { get; }

        /// <summary>
        /// Gets the set of items stored in this device.
        /// </summary>
        public RnetDataItemCollection Items { get; private set; }

    }

}
