using System.Threading.Tasks;

namespace Rnet.Drivers.Default
{

    /// <summary>
    /// Provides a default set of drivers for some basic device support.
    /// </summary>
    public sealed class DriverPackage : Drivers.DriverPackage
    {

        protected internal override Task<Driver> GetDriver(RnetDevice device)
        {
            if (device is RnetController)
                return Task.FromResult<Driver>(new ControllerDriver((RnetController)device));
            else if (device is RnetLocalDevice)
                return Task.FromResult<Driver>(new LocalDeviceDriver((RnetLocalDevice)device));
            return null;
        }

        public override DriverPriority Priority
        {
            get { return DriverPriority.Fallback; }
        }

    }

}
