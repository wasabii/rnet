namespace Rnet.Drivers
{

    /// <summary>
    /// Serves as a simple profile implementation base for a device.
    /// </summary>
    public abstract class DeviceProfileBase : ProfileBase
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        protected DeviceProfileBase(RnetDevice target)
            : base(target)
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
