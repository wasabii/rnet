namespace Rnet
{

    /// <summary>
    /// RNet "Device ID" structure.
    /// </summary>
    internal struct RnetDeviceId
    {

        /// <summary>
        /// <see cref="RnetDeviceId"/> which targets the controller.
        /// </summary>
        public static readonly RnetDeviceId RootControllerTarget = new RnetDeviceId(0, 0, (byte)RnetKeypadTargets.Controller);

        /// <summary>
        /// <see cref="RnetDeviceId"/> recommended by Russound for external control systems.
        /// </summary>
        public static readonly RnetDeviceId ExternalSource = new RnetDeviceId(0, 0, 0x70);

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="controllerId"></param>
        /// <param name="zoneId"></param>
        /// <param name="keypadId"></param>
        internal RnetDeviceId(byte controllerId, byte zoneId, byte keypadId)
            : this()
        {
            ControllerId = controllerId;
            ZoneId = zoneId;
            KeypadId = keypadId;
        }

        /// <summary>
        /// ID of the controller.
        /// </summary>
        public byte ControllerId { get; set; }

        /// <summary>
        /// ID of the zone.
        /// </summary>
        public byte ZoneId { get; set; }

        /// <summary>
        /// ID of the keypad.
        /// </summary>
        public byte KeypadId { get; set; }

    }

}
