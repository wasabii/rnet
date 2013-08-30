namespace Rnet.Drivers.Default
{

    /// <summary>
    /// Serves as a simple profile implementation base for a device.
    /// </summary>
    public abstract class DeviceBase : ProfileBase
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        protected DeviceBase(RnetDevice target)
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
