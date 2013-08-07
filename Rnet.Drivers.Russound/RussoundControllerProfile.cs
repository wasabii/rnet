using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Rnet.Profiles.Basic;

namespace Rnet.Profiles.Russound
{

    /// <summary>
    /// Implements the profiles supported against a Russound controller.
    /// </summary>
    class RussoundControllerProfile : ControllerProfileBase, IObject, IDevice, IController, IRussoundController
    {

        RnetZone[] zones;

        string name;

        RnetDataHandle modelHandle;
        string model;

        RnetDataHandle firmwareVersionHandle;
        string firmwareVersion;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="controller"></param>
        public RussoundControllerProfile(RnetController controller, int zoneCount)
            : base(controller)
        {
            Trace.TraceInformation("RussoundControllerProfile:ctor Controller={0}", controller.Id);

            // grab references to the supported zones
            zones = Enumerable.Range(0, zoneCount)
                .Select(i => Controller.Zones[i])
                .ToArray();

            modelHandle = controller[0, 0];
            firmwareVersionHandle = controller[0, 1];
        }

        protected override async Task InitializeAsync()
        {
            await modelHandle.Subscribe(d =>
                Name = Model = d);
            await firmwareVersionHandle.Subscribe(d =>
                FirmwareVersion = d);

            // ensure activation of all zones
            foreach (var zone in zones)
                zone.Activate();
        }

        public string Name
        {
            get { return name; }
            private set { name = value; RaisePropertyChanged("Name"); }
        }

        public string Manufacturer
        {
            get { return "Russound"; }
        }

        public string Model
        {
            get { return model; }
            private set { model = value; RaisePropertyChanged("Model"); }
        }

        public string FirmwareVersion
        {
            get { return firmwareVersion; }
            private set { firmwareVersion = value; RaisePropertyChanged("FirmwareVersion"); }
        }

        public RnetZone[] Zones
        {
            get { return zones; }
        }

    }

}
