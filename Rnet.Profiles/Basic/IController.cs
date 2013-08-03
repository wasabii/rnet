using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rnet.Profiles.Basic
{

    /// <summary>
    /// Describes basic capabilities of controllers.
    /// </summary>
    public interface IController : IDevice
    {

        /// <summary>
        /// Returns the total number of zones on the controller.
        /// </summary>
        /// <returns></returns>
        int ZoneCount { get; }

        /// <summary>
        /// Obtains the set of applicable profiles for the given zone.
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        Task<IEnumerable<IProfile>> GetZoneProfilesAsync(RnetZone zone);

    }

}
