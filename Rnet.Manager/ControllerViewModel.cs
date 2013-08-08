using System.Collections.Generic;
using System.Linq;

using OLinq;

namespace Rnet.Manager
{

    public class ControllerViewModel : BusObjectViewModel
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="controller"></param>
        public ControllerViewModel(RnetController controller)
            : base(controller)
        {
            // wrap zones in view model
            Zones = Controller.Zones
                .AsObservableQuery()
                .Select(i => new ZoneViewModel(i))
                .AsObservableQuery()
                .ToObservableView();
        }

        public RnetController Controller
        {
            get { return (RnetController)Target; }
        }

        /// <summary>
        /// Gets the zones within the controller.
        /// </summary>
        public IEnumerable<ZoneViewModel> Zones { get; private set; }

    }

}
