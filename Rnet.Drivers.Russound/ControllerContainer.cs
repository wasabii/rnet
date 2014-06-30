using System.Linq;
using System.Threading.Tasks;

using Rnet.Drivers.Russound.Media.Audio;

namespace Rnet.Drivers.Russound
{

    /// <summary>
    /// Provides a <see cref="IContainer"/> implementation for Russound controllers. At this point only addressable
    /// zone objects are provided.
    /// </summary>
    public class ControllerContainer : Default.ControllerContainer
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        internal ControllerContainer(RnetController controller)
            : base(controller)
        {

        }

        public override async Task<object[]> GetProfiles(RnetBusObject target)
        {
            return Enumerable.Concat(
                    await GetRussoundProfiles(target),
                    await base.GetProfiles(target))
                .ToArray();
        }

        /// <summary>
        /// Obtains additional profiles for a nested bus object.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        protected virtual Task<object[]> GetRussoundProfiles(RnetBusObject target)
        {
            var zone = target as RnetZone;
            if (zone == null)
                return null;

            // our zones only
            if (zone.Controller != Controller)
                return null;

            return Task.FromResult(new object[]
            { 
                new Zone(zone),
                new Media.Audio.Zone(zone),
            });
        }

    }

}
