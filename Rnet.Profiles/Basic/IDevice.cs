using System.Threading.Tasks;

namespace Rnet.Profiles.Basic
{

    /// <summary>
    /// Provides basic device information.
    /// </summary>
    public interface IDevice : IProfile
    {

        /// <summary>
        /// Gets the model of the device.
        /// </summary>
        /// <returns></returns>
        Task<string> GetModelAsync();

        /// <summary>
        /// Gets the manufacturer of the device.
        /// </summary>
        /// <returns></returns>
        Task<string> GetManufacturerAsync();

        /// <summary>
        /// Gets the firmware version of the device.
        /// </summary>
        /// <returns></returns>
        Task<string> GetFirmwareVersionAsync();

    }

}
