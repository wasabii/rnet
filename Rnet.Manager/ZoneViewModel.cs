using System.Collections.Generic;
using System.Linq;

using OLinq;

namespace Rnet.Manager
{

    public class ZoneViewModel : BusObjectViewModel
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="zone"></param>
        public ZoneViewModel(RnetZone zone)
            : base(zone)
        {
            // wrap devices in view model
            Devices = Zone.Devices
                .AsObservableQuery()
                .Select(i => new DeviceViewModel(i))
                .AsObservableQuery()
                .ToObservableView();
        }

        public RnetZone Zone
        {
            get { return (RnetZone)Object; }
        }

        /// <summary>
        /// Gets the devices within the zone.
        /// </summary>
        public IEnumerable<DeviceViewModel> Devices { get; private set; }

    }

}
