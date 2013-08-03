using System.Threading.Tasks;

using Rnet.Profiles.Capabilities;

namespace Rnet.Profiles.Devices
{

    public abstract class RussoundDevice : Device
    {


        RussoundDeviceInfo info;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="device"></param>
        internal RussoundDevice(RnetDevice device)
            : base(device)
        {
            info = new RussoundDeviceInfo(this);
        }

        public override DeviceInfo Info
        {
            get { return info; }
        }

    }

}
