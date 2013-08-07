using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rnet.Profiles;

namespace Rnet.Drivers.Default
{

    /// <summary>
    /// Basic controller driver.
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

        public override DriverPriority Priority
        {
            get { return DriverPriority.Fallback; }
        }

        protected override Task<object[]> GetProfiles()
        {
            return Task.FromResult<object[]>(new object[] 
            { 
                new ControllerProfile(Controller),
                new ControllerBusObjectContainer(Controller) 
            });
        }

    }

}
