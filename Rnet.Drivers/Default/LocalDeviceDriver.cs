using System.Threading.Tasks;

namespace Rnet.Drivers.Default
{

    /// <summary>
    /// Provides a fallback driver for the local device. If the local device is intended to be an actual implementation
    /// of an RNET device and not merely an endpoint used for communication, a driver implementation is a must.
    /// </summary>
    public class LocalDeviceDriver : Driver
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="device"></param>
        public LocalDeviceDriver(RnetLocalDevice device)
            : base(device)
        {

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
                new LocalDevice((RnetLocalDevice)Device),
            });
        }

    }

}
