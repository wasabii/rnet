using System.Collections.Generic;

namespace Rnet.Profiles
{

    /// <summary>
    /// Base interface for operating against a <see cref="RnetZone"/>.
    /// </summary>
    [Contract("urn:rnet:profiles", "Zone")]
    public interface IZone : IObject
    {

        /// <summary>
        /// Returns the set of physical devices underneath the zone.
        /// </summary>
        IEnumerable<RnetDevice> Devices { get; }

    }

}
