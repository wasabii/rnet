namespace Rnet
{

    /// <summary>
    /// Base class of RNET devices.
    /// </summary>
    public abstract class RnetDevice : RnetModelObject
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected RnetDevice()
        {
            DataItems = new RnetDataItemCollection();
        }

        /// <summary>
        /// Gets the ID of the device on the RNET bus.
        /// </summary>
        public abstract RnetDeviceId Id { get; }

        /// <summary>
        /// Gets the set of data items stored in this device.
        /// </summary>
        public RnetDataItemCollection DataItems { get; private set; }

    }

}
