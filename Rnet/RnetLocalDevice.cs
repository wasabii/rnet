using System;
using System.Diagnostics.Contracts;

namespace Rnet
{

    /// <summary>
    /// Reference to our own exposed device on the RNET bus.
    /// </summary>
    public sealed class RnetLocalDevice : RnetDevice
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        internal RnetLocalDevice(RnetZone zone, RnetKeypadId keypadId)
            : base(zone.Bus)
        {
            Contract.Requires<ArgumentNullException>(zone != null);

            if (keypadId >= 0x7c && keypadId <= 0x7f)
                throw new ArgumentOutOfRangeException("id", "RnetKeypadId cannot be in a reserved range.");
            
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
        /// Always returns <c>true</c>. A local device is always active.
        /// </summary>
        public override bool IsActive
        {
            // a local device is always active
            get { return true; }
        }

        /// <summary>
        /// has no effect. A local device is always active.
        /// </summary>
        public override void Activate()
        {

        }

        /// <summary>
        /// Creates a new data handle to the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected internal override RnetDataHandle CreateDataHandle(RnetPath path)
        {
            return new RnetLocalDataHandle(this, path);
        }

    }

}
