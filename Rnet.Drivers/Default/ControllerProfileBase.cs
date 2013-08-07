namespace Rnet.Drivers.Default
{

    /// <summary>
    /// Serves as a simple profile implementation base for a controller.
    /// </summary>
    public abstract class ControllerProfileBase : DeviceProfileBase
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="controller"></param>
        protected ControllerProfileBase(RnetController controller)
            : base(controller)
        {

        }

        protected new RnetController Device
        {
            get { return (RnetController)base.Device; }
        }

        protected RnetController Controller
        {
            get { return Device; }
        }

    }

}
