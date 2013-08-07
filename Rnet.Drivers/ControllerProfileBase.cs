namespace Rnet.Drivers
{

    /// <summary>
    /// Serves as a simple profile implementation base for a controller.
    /// </summary>
    public abstract class ControllerProfileBase : DeviceProfileBase
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        protected ControllerProfileBase(RnetController target)
            : base(target)
        {

        }

        /// <summary>
        /// The controller implementing this profile.
        /// </summary>
        protected RnetController Controller
        {
            get { return (RnetController)Device; }
        }

    }

}
