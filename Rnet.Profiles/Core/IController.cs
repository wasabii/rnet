using System.Collections.Generic;

namespace Rnet.Profiles.Core
{

    /// <summary>
    /// Basic interface for operating against a <see cref="RnetController"/>.
    /// </summary>
    [ProfileContract("core", "Controller")]
    public interface IController : IDevice
    {

        /// <summary>
        /// Returns the set of zones officially supported by the controller.
        /// </summary>
        IEnumerable<RnetZone> Zones { get; }

    }

}
