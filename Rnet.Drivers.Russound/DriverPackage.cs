using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Rnet.Drivers.Russound
{

    /// <summary>
    /// Provides Russound specific drivers for known Russound devices.
    /// </summary>
    public class DriverPackage :
        Drivers.DriverPackage
    {

        /// <summary>
        /// We provide official drivers for Russound.
        /// </summary>
        public override DriverPriority Priority
        {
            get { return DriverPriority.Default; }
        }

        protected override Task<Driver> GetDriver(RnetDevice device)
        {
            if (device is RnetController)
                return GetDriver((RnetController)device);

            return Task.FromResult<Driver>(null);
        }

        async Task<Driver> GetDriver(RnetController controller)
        {
            Contract.Requires<ArgumentNullException>(controller != null);

            var model = await controller[0, 0].ReadAsciiString();
            if (model == null)
                return null;

            switch (model)
            {
                case "CAM 6.6":
                    return new CAM66(controller);
                case "CAM 6.6 Rev2":
                    return new CAM66(controller);
                case "CAV 6.6":
                    return new CAV66(controller);
            }

            return null;
        }

    }

}
