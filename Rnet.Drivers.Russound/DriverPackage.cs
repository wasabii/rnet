using System.Threading.Tasks;

namespace Rnet.Drivers.Russound
{

    /// <summary>
    /// Provides Russound specific drivers for known Russound devices.
    /// </summary>
    public sealed class DriverPackage : Drivers.DriverPackage
    {

        /// <summary>
        /// Initializes the static instance.
        /// </summary>
        static DriverPackage()
        {
            // register ourselves if we can
            DriverManager.Register<DriverPackage>();
        }
        
        /// <summary>
        /// We provide official drivers for Russound.
        /// </summary>
        public override DriverPriority Priority
        {
            get { return DriverPriority.Default; }
        }

        protected internal override Task<Driver> GetDriver(RnetDevice device)
        {
            if (device is RnetController)
                return GetDriver((RnetController)device);

            return null;
        }

        async Task<Driver> GetDriver(RnetController controller)
        {
            var model = await controller[0, 0].ReadAsciiString();
            if (model == null)
                return null;

            switch (model)
            {
                case "CAM 6.6":
                    return new CAM66(controller);
                case "CAV 6.6":
                    return new CAV66(controller);
            }

            return null;
        }

    }

}
