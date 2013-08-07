using System.Collections.Generic;

using Rnet.Profiles;

namespace Rnet.Drivers.Default
{

    /// <summary>
    /// Basic profiles for default controller driver.
    /// </summary>
    class ControllerProfile : ControllerProfileBase, IController, IDevice, IObject
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="controller"></param>
        public ControllerProfile(RnetController controller)
            : base(controller)
        {

        }

        public IEnumerable<RnetZone> Zones
        {
            get { return Controller.Zones; }
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

        public string DisplayName
        {
            get { return "Controller " + (Controller.Id.Value + 1); }
        }

    }

}
