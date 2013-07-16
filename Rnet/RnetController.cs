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
            : base(bus, new RnetDeviceId(controllerId, 0, RnetKeypadId.Controller))
        {

        }

        /// <summary>
        /// Gets the controller ID.
        /// </summary>
        public RnetControllerId ControllerId
        {
            get { return Id.ControllerId; }
        }

        /// <summary>
        /// Returns a string representation of the current instance.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Controller: {0}", ControllerId);
        }

    }

}
