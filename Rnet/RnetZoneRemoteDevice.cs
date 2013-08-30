namespace Rnet
{

    /// <summary>
    /// Describes a remote device that falls under a zone. These are assigned a zone and keypad id.
    /// </summary>
    public class RnetZoneRemoteDevice : RnetRemoteDevice
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="keypadId"></param>
        internal RnetZoneRemoteDevice(RnetZone zone, RnetKeypadId keypadId)
            : base(zone.Controller.Bus)
        {
            Zone = zone;
            Id = keypadId;
        }

        /// <summary>
        /// Gets the zone in which this device resides.
        /// </summary>
        public RnetZone Zone { get; private set; }

        /// <summary>
        /// Get the keypad idea of the device.
        /// </summary>
        public RnetKeypadId Id { get; private set; }

        /// <summary>
        /// Gets the ID of the device on the RNET bus.
        /// </summary>
        public override RnetDeviceId DeviceId
        {
            get { return new RnetDeviceId(Zone.Controller.Id, Zone.Id, Id); }
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
                Zone.Devices.OnDeviceActive(this);
        }

    }

}
