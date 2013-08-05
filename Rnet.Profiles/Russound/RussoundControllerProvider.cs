using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rnet.Profiles.Basic;

namespace Rnet.Profiles.Russound
{

    /// <summary>
    /// Basic Russound controller profile. Provides functionality common to all Russound controllers.
    /// </summary>
    public abstract class RussoundControllerProvider : ControllerProfileProvider
    {

        class ControllerProfile : ControllerProfileBase, IObject, IDevice, IController, IRussoundController
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
            public ControllerProfile(RnetController controller, int zoneCount)
                : base(controller)
            {
                zones = Enumerable.Range(0, zoneCount).Select(i => Controller.Zones[i]).ToArray();
                modelHandle = controller[0, 0];
                firmwareVersionHandle = controller[0, 1];
            }

            protected override async Task InitializeAsync()
            {
                await modelHandle.Subscribe(d =>
                    name = model = d);
                await firmwareVersionHandle.Subscribe(d =>
                    firmwareVersion = d);
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

            public Task<IEnumerable<IProfile>> GetZoneProfilesAsync(RnetZone zone)
            {
                if (Controller != zone.Controller)
                    return null;

                return Task.FromResult((IEnumerable<IProfile>)new IProfile[]
                { 
                    new ControllerZoneProfile(zone),
                    new RussoundZoneAudioProfile(zone),
                });
            }

        }

        class ControllerZoneProfile : ZoneProfileBase, IObject, IZone, IRussoundZone
        {

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="zone"></param>
            public ControllerZoneProfile(RnetZone zone)
                : base(zone)
            {

            }

            public string Name
            {
                get { return "Zone " + Zone.Id.Value; }
                set { }
            }

        }

        /// <summary>
        /// Implement this method to test a given model number.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        protected abstract bool IsSupportedModel(string model);

        /// <summary>
        /// Implement this method to return the number of supported zones.
        /// </summary>
        protected abstract int ZoneCount { get; }

        /// <summary>
        /// Gets the applicable profiles for the specified device.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public override async Task<IEnumerable<IProfile>> GetControllerProfilesAsync(RnetController controller)
        {
            var model = await controller[0, 0].ReadAsciiString();
            if (model == null)
                return null;

            if (IsSupportedModel(model))
                return new IProfile[]
                { 
                    new ControllerProfile(controller, ZoneCount),
                    new RussoundControllerAudioProfile(controller),
                };

            return null;
        }

    }

}
