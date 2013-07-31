using System;

namespace Rnet
{

    /// <summary>
    /// Device node representing the bus itself.
    /// </summary>
    public sealed class RnetBusDevice : RnetZoneDevice
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetBusDevice(RnetZone zone, RnetKeypadId keypadId)
            : base(zone, keypadId)
        {
            if (keypadId >= 0x7c && keypadId <= 0x7f)
                throw new ArgumentOutOfRangeException("id", "RnetKeypadId cannot be in a reserved range.");

            Visible = true;
            Model = "Bus";
        }

    }

}
