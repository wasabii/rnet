using System.Threading.Tasks;

namespace Rnet.Drivers.Default
{


    public sealed class DefaultDriverPackage : DriverPackage
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
