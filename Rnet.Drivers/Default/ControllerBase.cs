using System;
using System.Diagnostics.Contracts;

namespace Rnet.Drivers.Default
{

    /// <summary>
    /// Serves as a simple profile implementation base for a controller.
    /// </summary>
    public abstract class ControllerBase : DeviceBase
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="controller"></param>
        protected ControllerBase(RnetController controller)
            : base(controller)
        {
            Contract.Requires<ArgumentNullException>(controller != null);
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
