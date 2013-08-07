using System.ServiceModel;

namespace Rnet.Profiles.Basic
{

    /// <summary>
    /// Describes basic capabilities of controllers.
    /// </summary>
    [ServiceContract]
    public interface IController : Driver
    {

        /// <summary>
        /// Gets the zones supported by the controller.
        /// </summary>
        RnetZone[] Zones { get; }

    }

}
