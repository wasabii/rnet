namespace Rnet
{

    /// <summary>
    /// Describes a zone underneath an RNET controller.
    /// </summary>
    public sealed class RnetZone : RnetBusObject
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        internal RnetZone(RnetController controller, RnetZoneId id)
        {
            Controller = controller;
            Id = id;
            Devices = new RnetZoneDeviceCollection(this);
        }

        /// <summary>
        /// Controller that manages this zone.
        /// </summary>
        public RnetController Controller { get; private set; }

        /// <summary>
        /// Gets the zone ID of the zone.
        /// </summary>
        public RnetZoneId Id { get; private set; }

        /// <summary>
        /// The set of devices underneath this zone.
        /// </summary>
        public RnetZoneDeviceCollection Devices { get; private set; }

        /// <summary>
        /// Marks the object as active.
        /// </summary>
        /// <returns></returns>
        internal override void Touch()
        {
            var a = !IsActive;
            base.Touch();
            if (a)
                Controller.Zones.OnZoneActive(this);
        }

        public override string ToString()
        {
            return string.Format("Zone {0}", Id);
        }

    }

}
