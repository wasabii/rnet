using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rnet.Profiles.Russound
{

    /// <summary>
    /// Basic Russound controller profile. Provides functionality common to all Russound controllers.
    /// </summary>
    public abstract class RussoundControllerProvider : ControllerProfileProvider
    {

        /// <summary>
        /// Implement this method to test whether a given model value is supported by the implemeneted Russound
        /// controller provider.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        protected abstract bool IsSupportedModel(string model);

        /// <summary>
        /// Implement this method to return the number of supported zones for the specific Russound device model.
        /// </summary>
        protected abstract int ZoneCount { get; }

        /// <summary>
        /// Test whether or not the controller is supported.
        /// </summary>
        /// <returns></returns>
        async Task<bool> TestSupported(RnetController controller)
        {
            var model = await controller[0, 0].ReadAsciiString();
            if (model == null)
                return false;

            return IsSupportedModel(model);
        }

        /// <summary>
        /// Gets the applicable profiles for the specified device.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public override async Task<IEnumerable<Driver>> GetControllerProfiles(RnetController controller)
        {
            if (await TestSupported(controller))
                return new Driver[]
                { 
                    new RussoundControllerProfile(controller, ZoneCount),
                    new RussoundControllerAudioProfile(controller),
                };

            return null;
        }

        /// <summary>
        /// Gets the applicable profiles for the specified zone.
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public override async Task<IEnumerable<Driver>> GetZoneProfiles(RnetZone zone)
        {
            if (await TestSupported(zone.Controller))
                return new Driver[]
                {
                    new RussoundZoneProfile(zone),
                    new RussoundZoneAudioProfile(zone),
                };

            return null;
        }

    }

}
