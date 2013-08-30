using System.Collections.Generic;

using Rnet.Profiles;

namespace Rnet.Drivers.Default
{

    /// <summary>
    /// Provides a default implementation of 
    /// </summary>
    public class Controller : ControllerBase, IController
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="controller"></param>
        public Controller(RnetController controller)
            : base(controller)
        {

        }

        public string Id
        {
            get { return "controller-" + (Controller.DeviceId.ControllerId + 1); }
        }

        public string DisplayName
        {
            get { return "Controller " + (Controller.DeviceId.ControllerId + 1); }
        }

        public string Manufacturer
        {
            get { return "Unknown"; }
        }

        public string Model
        {
            get { return "Unknown"; }
        }

        public string FirmwareVersion
        {
            get { return "Unknown"; }
        }

        public IEnumerable<RnetZone> Zones
        {
            get { return Controller.Zones; }
        }

    }

}
