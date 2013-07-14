namespace Rnet
{

    /// <summary>
    /// Describes a known controller on the RNET bus.
    /// </summary>
    public sealed class RnetController : RnetDevice
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="controllerId"></param>
        public RnetController(RnetBus bus, RnetControllerId controllerId)
            : base(bus)
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
