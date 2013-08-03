namespace Rnet.Profiles
{

    /// <summary>
    /// Serves as a simple profile implementation base for a device.
    /// </summary>
    public abstract class DeviceProfileObject : ProfileObject
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="device"></param>
        protected DeviceProfileObject(RnetDevice device)
            : base(device)
        {

        }

        /// <summary>
        /// The device implementing this profile.
        /// </summary>
        protected RnetDevice Device
        {
            get { return (RnetDevice)Target; }
        }

    }

}
