using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Rnet.Profiles.Basic
{

    /// <summary>
    /// Describes basic capabilities of controllers.
    /// </summary>
    [ServiceContract]
    public interface IController : IDevice
    {

        /// <summary>
        /// Returns the total number of zones on the controller.
        /// </summary>
        /// <returns></returns>
        int ZoneCount { [OperationContract] get; }

        /// <summary>
        /// Obtains the set of applicable profiles for the given zone.
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        [OperationContract]
        Task<IEnumerable<IProfile>> GetZoneProfilesAsync(RnetZone zone);

    }

}
