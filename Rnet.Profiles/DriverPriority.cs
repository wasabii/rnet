namespace Rnet.Drivers
{

    /// <summary>
    /// Relative priority for a driver.
    /// </summary>
    public enum DriverPriority
    {

        /// <summary>
        /// Available if no other driver is offered.
        /// </summary>
        Fallback = -256,

        /// <summary>
        /// Default position of new drivers. Set to Native if you are providing an official driver.
        /// </summary>
        Unconfigured = -128,

        /// <summary>
        /// Official driver for the device. Supercedes default drivers if available.
        /// </summary>
        Native = 0,

        /// <summary>
        /// Custom driver which supercedes native drivers if available.
        /// </summary>
        Override = 256,

    }

}
