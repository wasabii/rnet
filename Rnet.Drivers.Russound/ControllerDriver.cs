using System.Linq;
using System.Threading.Tasks;

namespace Rnet.Drivers.Russound
{

    /// <summary>
    /// Basic Russound controller driver. Provides functionality common to all Russound controllers.
    /// </summary>
    public abstract class ControllerDriver : Default.ControllerDriver
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="controller"></param>
        public ControllerDriver(RnetController controller)
            : base(controller)
        {

        }

        /// <summary>
        /// Implement this method to return the number of supported zones for the specific Russound device model.
        /// </summary>
        protected abstract int ZoneCount { get; }

        /// <summary>
        /// Returns a set of profiles compatible with Russound controllers.
        /// </summary>
        /// <returns></returns>
        protected override async Task<object[]> GetProfiles()
        {
            return Enumerable.Concat(
                    await GetRussoundProfiles(),
                    await base.GetProfiles())
                .ToArray();
        }

        /// <summary>
        /// Returns a set of profiles specific to Russound controllers.
        /// </summary>
        /// <returns></returns>
        protected internal Task<object[]> GetRussoundProfiles()
        {
            return Task.FromResult(new object[]
            { 
                new Controller(Controller, ZoneCount),
                new ControllerContainer(Controller),
            });
        }

    }

}
