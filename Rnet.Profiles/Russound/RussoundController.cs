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

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="controller"></param>
            public ControllerProfile(RnetController controller)
                : base(controller)
            {

            }

            public Task<string> GetModelAsync()
            {
                return Device.Directory.GetAsciiStringAsync(0, 0);
            }

            public Task<string> GetManufacturerAsync()
            {
                return Task.FromResult("Russound");
            }

            public Task<string> GetFirmwareVersionAsync()
            {
                return Device.Directory.GetAsciiStringAsync(0, 1);
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
            var model = await controller.Directory.GetAsciiStringAsync(0, 0);
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
