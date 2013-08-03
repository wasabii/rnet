namespace Rnet
{

    /// <summary>
    /// Base class of devices underneath a zone.
    /// </summary>
    public class RnetZoneDevice : RnetDevice
    {

        public override RnetDeviceId DeviceId
        {
            get { return new RnetDeviceId(Zone.Controller.Id, Zone.Id, Id); }
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="controllerId"></param>
        /// <param name="zoneId"></param>
        /// <param name="keypadId"></param>
        internal RnetZoneDevice(RnetZone zone, RnetKeypadId keypadId)
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

    }

}
