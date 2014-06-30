using Rnet.Profiles.Core;

namespace Rnet.Profiles.Media
{

    /// <summary>
    /// Basic media information for a zone.
    /// </summary>
    [ProfileContract("media", "Zone")]
    public interface IZone
    {

        /// <summary>
        /// Whether the zone is powered on or off.
        /// </summary>
        Power Power { get; set; }

        /// <summary>
        /// Toggles the power.
        /// </summary>
        void PowerToggle();

        /// <summary>
        /// Turns the power off.
        /// </summary>
        void PowerOn();

        /// <summary>
        /// Turns the power on.
        /// </summary>
        void PowerOff();

        /// <summary>
        /// Gets or sets the numeric index of the source input.
        /// </summary>
        int Source { get; set; }

    }

}
