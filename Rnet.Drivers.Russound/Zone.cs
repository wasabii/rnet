using System.Threading.Tasks;

namespace Rnet.Drivers.Russound
{

    /// <summary>
    /// Provides an implementation of <see cref="IZone"/> for a Russound zone.
    /// </summary>
    public class Zone : Default.Zone
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="zone"></param>
        public Zone(RnetZone zone)
            : base(zone)
        {

        }

        protected override async Task Initialize()
        {
            await base.Initialize();
        }

    }

}
