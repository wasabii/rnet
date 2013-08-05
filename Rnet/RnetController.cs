namespace Rnet
{

    /// <summary>
    /// Describes a known controller on the RNET bus.
    /// </summary>
    public sealed class RnetController : RnetDevice
    {

        public override RnetDeviceId DeviceId
        {
            get { return new RnetDeviceId(Id, 0, RnetKeypadId.Controller); }
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="id"></param>
        public RnetController(RnetBus bus, RnetControllerId id)
            : base(bus)
        {
            Id = id;
            Zones = new RnetZoneCollection(this);
        }

        /// <summary>
        /// Gets the controller ID.
        /// </summary>
        public RnetControllerId Id { get; private set; }

        /// <summary>
        /// Zones available underneath this controller.
        /// </summary>
        public RnetZoneCollection Zones { get; private set; }

        /// <summary>
        /// Marks the object as active.
        /// </summary>
        /// <returns></returns>
        public override void Activate()
        {
            var a = !IsActive;
            base.Activate();
            if (a)
                Bus.Controllers.OnControllerActive(this);
        }

        public override string ToString()
        {
            return string.Format("Controller {0}", Id);
        }

    }

}
