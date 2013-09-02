using System.Threading.Tasks;

namespace Rnet.Profiles.Core
{

    /// <summary>
    /// Provides functionality for devices that own other bus objects. This is usually implemented along with
    /// <see cref="IContainer"/> to provide information to nested objects.
    /// </summary>
    [ProfileContract("core", "Owner")]
    public interface IOwner
    {

        /// <summary>
        /// Obtains the profile implementations of the requested type for the given nested <see cref="RnetBusObject"/>.
        /// This method is used internally. Users interested in obtaining a profile implementation for a nested device
        /// should use the provided extension method of the driver manager.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        Task<object[]> GetProfiles(RnetBusObject target);

    }

}
