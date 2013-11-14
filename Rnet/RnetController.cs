namespace Rnet
{

    /// <summary>
    /// Reference to an RNET controller present on the RNET bus.
    /// </summary>
    public sealed class RnetController : RnetRemoteDevice
    {

        readonly RnetControllerId id;
        readonly RnetZoneCollection zones;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="id"></param>
        public RnetController(RnetBus bus, RnetControllerId id)
            : base(bus)
        {
            RnetTraceSource.Default.TraceInformation("RnetController:ctor Id={0}", id);

            this.id = id;
            this.zones = new RnetZoneCollection(this);
        }

        /// <summary>
        /// Gets the controller ID.
        /// </summary>
        public RnetControllerId Id
        {
            get { return id; }
        }

        /// <summary>
        /// Zones available underneath this controller.
        /// </summary>
        public RnetZoneCollection Zones
        {
            get { return zones; }
        }

        /// <summary>
        /// Gets the device ID of the controller.
        /// </summary>
        public override RnetDeviceId DeviceId
        {
            get { return new RnetDeviceId(Id, 0, RnetKeypadId.Controller); }
        }

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

    }

}
