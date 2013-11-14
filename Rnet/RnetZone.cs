using System;
using System.Diagnostics.Contracts;

namespace Rnet
{

    /// <summary>
    /// Describes a zone underneath an RNET controller.
    /// </summary>
    public sealed class RnetZone : RnetBusObject
    {

        readonly RnetController controller;
        readonly RnetZoneId id;
        readonly RnetZoneDeviceCollection devices;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        internal RnetZone(RnetController controller, RnetZoneId id)
            : base(controller.Bus)
        {
            Contract.Requires<ArgumentNullException>(controller != null);
            RnetTraceSource.Default.TraceInformation("RnetZone:ctor Id={0}", (int)id);

            this.controller = controller;
            this.id = id;
            this.devices = new RnetZoneDeviceCollection(this);
        }

        /// <summary>
        /// Controller that manages this zone.
        /// </summary>
        public RnetController Controller
        {
            get { return controller; }
        }

        /// <summary>
        /// Gets the zone ID of the zone.
        /// </summary>
        public RnetZoneId Id
        {
            get { return id; }
        }

        /// <summary>
        /// The set of devices underneath this zone.
        /// </summary>
        public RnetZoneDeviceCollection Devices
        {
            get { return devices; }
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
                Controller.Zones.OnZoneActive(this);
        }

        public override string ToString()
        {
            return string.Format("Zone {0}", Id);
        }

    }

}
