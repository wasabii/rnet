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
        Fallback = 256,

        /// <summary>
        /// Default unconfigured priority of drivers. Prevents superseding Default drivers if they are available.
        /// </summary>
        Unconfigured = 128,

        /// <summary>
        /// Default driver for the device. Must be set explicitely to override the default position of Unconfigured.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Custom driver which supercedes Default drivers if available.
        /// </summary>
        Override = -256,

    }

}
