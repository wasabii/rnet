using System.ServiceModel;

namespace Rnet.Profiles
{

    /// <summary>
    /// Provides basic device information.
    /// </summary>
    [ServiceContract(Name = "device")]
    public interface IDevice 
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
