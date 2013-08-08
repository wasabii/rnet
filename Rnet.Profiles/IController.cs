using System.Collections.Generic;
using System.ServiceModel;

namespace Rnet.Profiles
{

    /// <summary>
    /// Basic interface for operating against a <see cref="RnetController"/>.
    /// </summary>
    [ServiceContract(Name = "controller")]
    public interface IController : IDevice
    {

        /// <summary>
        /// Returns the set of zones officially supported by the controller.
        /// </summary>
        IEnumerable<RnetZone> Zones { get; }

    }

}
