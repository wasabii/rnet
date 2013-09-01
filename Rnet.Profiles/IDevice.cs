using System.Xml.Serialization;

namespace Rnet.Profiles
{

    /// <summary>
    /// Provides basic device information.
    /// </summary>
    [ProfileContract("Device")]
    [XmlRoot(Namespace = "urn:rnet:profiles::Device", ElementName = "Device")]
    public interface IDevice : IObject
    {

        /// <summary>
        /// Gets the manufacturer of the device.
        /// </summary>
        /// <returns></returns>
        string Manufacturer { get; }

        /// <summary>
        /// Gets the model of the device.
        /// </summary>
        /// <returns></returns>
        string Model { get; }

        /// <summary>
        /// Gets the firmware version of the device.
        /// </summary>
        /// <returns></returns>
        string FirmwareVersion { get; }

    }

}
