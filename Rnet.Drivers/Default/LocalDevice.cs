using Rnet.Profiles;

namespace Rnet.Drivers.Default
{

    /// <summary>
    /// Serves as a simple profile implementation base for the local device.
    /// </summary>
    public sealed class LocalDevice : ProfileBase, IDevice
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="device"></param>
        public LocalDevice(RnetLocalDevice device)
            : base(device)
        {

        }

        protected new RnetLocalDevice Target
        {
            get { return (RnetLocalDevice)base.Target; }
        }

        public string Id
        {
            get { return "bus-" + (int)Target.DeviceId.KeypadId; }
        }

        public string Manufacturer
        {
            get { return "Rnet"; }
        }

        public string Model
        {
            get { return "Bus Communication Device"; }
        }

        public string FirmwareVersion
        {
            get { return typeof(LocalDevice).Assembly.GetName().Version.ToString(); }
        }

        public string DisplayName
        {
            get { return "Bus Communication Device"; }
        }

    }

}
