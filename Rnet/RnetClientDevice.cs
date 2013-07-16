using System;

namespace Rnet
{

    /// <summary>
    /// Client device node representing ourselves.
    /// </summary>
    public sealed class RnetClientDevice : RnetDevice
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetClientDevice(RnetBus bus, RnetDeviceId id)
            : base(bus, id)
        {
            if (id.KeypadId >= 0x7c && id.KeypadId <= 0x7f)
                throw new ArgumentOutOfRangeException("id", "RnetKeypadId cannot be in a reserved range.");

            Visible = true;
            ModelName = "Client";
        }

    }

}
