namespace Rnet.Profiles.Media
{

    /// <summary>
    /// Basic audio information for a controller.
    /// </summary>
    public interface IControllerAudio : IProfile
    {

        /// <summary>
        /// Whether all zones are powered on or off.
        /// </summary>
        Power Power { get; set; }

    }

}
