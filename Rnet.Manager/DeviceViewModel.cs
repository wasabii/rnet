namespace Rnet.Manager
{

    public class DeviceViewModel : BusObjectViewModel
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="device"></param>
        public DeviceViewModel(RnetDevice device)
            : base(device)
        {

        }

        public RnetDevice Device
        {
            get { return (RnetDevice)Target; }
        }

    }

}
