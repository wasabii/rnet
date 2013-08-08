using System.Collections.Generic;
using System.ServiceModel;

namespace Rnet.Profiles
{

    /// <summary>
    /// Base interface for operating against a <see cref="RnetZone"/>.
    /// </summary>
    [ServiceContract(Name = "zone")]
    public interface IZone : IObject
    {

        /// <summary>
        /// Returns the set of physical devices underneath the zone.
        /// </summary>
        IEnumerable<RnetDevice> Devices { get; }

    }

}
