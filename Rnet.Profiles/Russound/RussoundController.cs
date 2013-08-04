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

            string model;
            string firmwareVersion;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="controller"></param>
            public ControllerProfile(RnetController controller)
                : base(controller)
            {

            }

            protected override async Task InitializeAsync()
            {
                Model = await Device.Root.GetAsciiStringAsync(0, 0);
                FirmwareVersion = await Device.Root.GetAsciiStringAsync(0, 1);
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
            var model = await controller.Root.GetAsciiStringAsync(0, 0);
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
