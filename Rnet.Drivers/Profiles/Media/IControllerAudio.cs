using Rnet.Profiles.Basic;

namespace Rnet.Profiles.Media
{

    /// <summary>
    /// Basic audio information for a controller.
    /// </summary>
    public interface IControllerAudio : Driver
    {

        /// <summary>
        /// Whether all zones are powered on or off.
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

    }

}
