using System.Threading.Tasks;

namespace Rnet.Profiles.Basic
{

    /// <summary>
    /// Provides basic device information.
    /// </summary>
    public interface IDevice : IProfile
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
