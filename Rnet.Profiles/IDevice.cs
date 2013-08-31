namespace Rnet.Profiles
{

    /// <summary>
    /// Provides basic device information.
    /// </summary>
    [Contract("urn:rnet:profiles", "Device")]
    public interface IDevice : IObject
    {

        /// <summary>
        /// Gets the manufacturer of the device.
        /// </summary>
        /// <returns></returns>
        [Value("Manufacturer")]
        string Manufacturer { get; }

        /// <summary>
        /// Gets the model of the device.
        /// </summary>
        /// <returns></returns>
        [Value("Model")]
        string Model { get; }

        /// <summary>
        /// Gets the firmware version of the device.
        /// </summary>
        /// <returns></returns>
        [Value("FirmwareVersion")]
        string FirmwareVersion { get; }

    }

}
