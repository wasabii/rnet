using System.Threading.Tasks;

namespace Rnet.Drivers.Default
{

    /// <summary>
    /// Provides the default drivers.
    /// </summary>
    public sealed class DriverPackage : Drivers.DriverPackage
    {

        protected internal override Task<Driver> GetDriver(RnetDevice device)
        {
            if (device is RnetController)
                return Task.FromResult<Driver>(new ControllerDriver((RnetController)device));
            else
                return null;
        }

    }

}
