using System.Threading.Tasks;

using Rnet.Profiles.Capabilities;

namespace Rnet.Profiles.Devices
{

    /// <summary>
    /// Provides information for Russound devices.
    /// </summary>
    class RussoundDeviceInfo : DeviceInfo
    {

        RussoundDevice device;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="device"></param>
        internal RussoundDeviceInfo(RussoundDevice device)
        {
            this.device = device;
        }

        public override string Model
        {
            get { throw new System.NotImplementedException(); }
        }

        public override string Manufacturer
        {
            get { throw new System.NotImplementedException(); }
        }

        public override string FirmwareVersion
        {
            get { throw new System.NotImplementedException(); }
        }

        public override Task WaitAsync(System.Threading.CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public override Task RefreshAsync(System.Threading.CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

    }

}
