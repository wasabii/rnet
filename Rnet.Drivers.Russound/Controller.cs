using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Rnet.Drivers.Default;
using Rnet.Profiles.Core;

namespace Rnet.Drivers.Russound
{

    /// <summary>
    /// Implements the <see cref="IController"/> profile for a Russound controller.
    /// </summary>
    public class Controller : ControllerBase, IController
    {

        RnetZone[] zones;

        string displayName;

        RnetDataHandle modelHandle;
        string model;

        RnetDataHandle firmwareVersionHandle;
        string firmwareVersion;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="zoneCount"></param>
        public Controller(RnetController controller, int zoneCount)
            : base(controller)
        {
            Contract.Requires<ArgumentNullException>(controller != null);
            Contract.Requires<ArgumentNullException>(zoneCount >= 1 && zoneCount <= 32);

            // grab references to the supported zones
            zones = Enumerable.Range(0, zoneCount)
                .Select(i => Controller.Zones[i])
                .ToArray();

            modelHandle =
                controller[0, 0];
            firmwareVersionHandle =
                controller[0, 1];
        }

        protected override Task Initialize()
        {
            modelHandle.ToAscii()
                .Subscribe(s =>
                    DisplayName = Model = s);

            firmwareVersionHandle.ToAscii()
                .Subscribe(d =>
                    FirmwareVersion = d);

            // ensure activation of all known zones
            foreach (var zone in zones)
                zone.Activate();

            return Task.FromResult(false);
        }

        public string Id
        {
            get { return "controller-" + (Controller.DeviceId.ControllerId + 1); }
        }

        public IEnumerable<RnetZone> Zones
        {
            get { return zones; }
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

        public string DisplayName
        {
            get { return displayName; }
            private set { displayName = value; RaisePropertyChanged("DisplayName"); }
        }

    }

}
