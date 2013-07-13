using Rnet.Protocol;

namespace Rnet.Model
{

    /// <summary>
    /// Describes a known controller on the RNET bus.
    /// </summary>
    public sealed class Controller : Device
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="controllerId"></param>
        public Controller(RnetControllerId controllerId)
        {
            ControllerId = controllerId;
        }

        public override RnetDeviceId Id
        {
            get { return new RnetDeviceId(ControllerId, 0, RnetKeypadId.Controller); }
        }

        /// <summary>
        /// Gets the controller ID.
        /// </summary>
        public RnetControllerId ControllerId { get; private set; }

    }

}
