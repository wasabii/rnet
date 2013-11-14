using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Rnet.Drivers.Default
{

    /// <summary>
    /// Basic controller driver. Can serve as the base for a more complicated implementation.
    /// </summary>
    public class ControllerDriver : Driver
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="controller"></param>
        public ControllerDriver(RnetController controller)
            : base(controller)
        {
            Contract.Requires<ArgumentNullException>(controller != null);
        }

        /// <summary>
        /// Returns the <see cref="RnetController"/> associated with this driver.
        /// </summary>
        public new RnetController Device
        {
            get { return (RnetController)base.Device; }
        }

        /// <summary>
        /// Returns the <see cref="RnetController"/> associated with this driver.
        /// </summary>
        public RnetController Controller
        {
            get { return Device; }
        }

        /// <summary>
        /// Default driver serves only as a fallback.
        /// </summary>
        public override DriverPriority Priority
        {
            get { return DriverPriority.Fallback; }
        }

        /// <summary>
        /// Returns a set of profiles which provide only basic functionality.
        /// </summary>
        /// <returns></returns>
        protected override Task<object[]> GetProfiles()
        {
            return Task.FromResult(new object[] 
            { 
                new Controller(Controller),
                new ControllerContainer(Controller) 
            });
        }

    }

}
