namespace Rnet.Profiles.Basic
{

    /// <summary>
    /// Obtains basic device information.
    /// </summary>
    public interface IDevice : IProfile
    {

        /// <summary>
        /// Provides the device information.
        /// </summary>
        DeviceInfo Info { get; }

    }

}
