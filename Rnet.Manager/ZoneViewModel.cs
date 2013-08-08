using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
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
            get { return (RnetZone)Target; }
        }

        /// <summary>
        /// Gets the devices within the zone.
        /// </summary>
        public ObservableView<DeviceViewModel> Devices { get; private set; }

    }

}
