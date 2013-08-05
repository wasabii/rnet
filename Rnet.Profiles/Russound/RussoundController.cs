using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rnet.Profiles.Russound
{

    /// <summary>
    /// Basic Russound controller profile. Provides functionality common to all Russound controllers.
    /// </summary>
    public abstract class RussoundController : ControllerProfileProvider
    {

        class ControllerProfile : ControllerProfileObject, IRussoundController
        {

            RnetDataHandle modelHandle;
            string model;

            RnetDataHandle firmwareVersionHandle;
            string firmwareVersion;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="controller"></param>
            public ControllerProfile(RnetController controller)
                : base(controller)
            {
                modelHandle = controller[0, 0];
                firmwareVersionHandle = controller[0, 1];
            }

            protected override async Task InitializeAsync()
            {
                await modelHandle.Subscribe(d =>
                    Model = d);
                await firmwareVersionHandle.Subscribe(d =>
                    FirmwareVersion = d);
            }

            public string Manufacturer
            {
                get { return "Russound"; }
            }

            public string Model
            {
                get { return model; }
                set { model = value; RaisePropertyChanged("Model"); }
            }

            public string FirmwareVersion
            {
                get { return firmwareVersion; }
                set { firmwareVersion = value; RaisePropertyChanged("FirmwareVersion"); }
            }

            public int ZoneCount
            {
                get { return 6; }
            }

            public Task<IEnumerable<IProfile>> GetZoneProfilesAsync(RnetZone zone)
            {
                if (zone.Controller != Device)
                    return null;

                if (zone.Id >= ZoneCount)
                    return null;

                return Task.FromResult((IEnumerable<IProfile>)new IProfile[]
                { 
                    new ZoneProfile(zone),
                    new RussoundZoneAudio(zone),
                });
            }

        }

        class ZoneProfile : ZoneProfileObject, IRussoundZone
        {

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="zone"></param>
            public ZoneProfile(RnetZone zone)
                : base(zone)
            {

            }

            public string Name
            {
                get { return null; }
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
                return new[]
                { 
                    new ControllerProfile(controller),
                };

            return null;
        }

    }

}
