namespace Rnet.Drivers.Default
{

    /// <summary>
    /// Serves as a simple profile implementation base for the local device.
    /// </summary>
    public abstract class LocalDeviceProfileBase : ProfileBase
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        protected LocalDeviceProfileBase(RnetLocalDevice target)
            : base(target)
        {

        }

        /// <summary>
        /// The device implementing this profile.
        /// </summary>
        protected RnetLocalDevice Device
        {
            get { return (RnetLocalDevice)Target; }
        }

    }

}
