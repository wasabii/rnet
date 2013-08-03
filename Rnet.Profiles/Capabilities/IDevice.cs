namespace Rnet.Profiles.Capabilities
{

    /// <summary>
    /// Obtains basic device information.
    /// </summary>
    public interface IDevice : IProfile
    {

        /// <summary>
        /// Obtains a copy of the basic device information.
        /// </summary>
        /// <returns></returns>
        DeviceInfo Info { get; }

    }

}
