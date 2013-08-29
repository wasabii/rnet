using System;
using System.Threading.Tasks;
namespace Rnet.Drivers.Russound
{

    /// <summary>
    /// Provides a 
    /// </summary>
    public class Zone : Default.Zone
    {

        RnetDataHandle runHandle;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="zone"></param>
        public Zone(RnetZone zone)
            : base(zone)
        {
            runHandle =
                Zone.Controller[4, 3, 0, 3];
        }

        protected override async Task Initialize()
        {
            await base.Initialize();

            runHandle
                .ToAscii()
                .Subscribe(d => Console.WriteLine(d));
        }

    }

}
