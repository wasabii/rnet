using System.Threading.Tasks;

namespace Rnet.Profiles.Media
{

    /// <summary>
    /// Basic audio information for a zone.
    /// </summary>
    public interface IZoneAudio : IProfile
    {

        /// <summary>
        /// Whether the zone is powered on or off.
        /// </summary>
        Power Power { get; set; }

        /// <summary>
        /// Volume level of the zone.
        /// </summary>
        int Volume { get; set; }

        /// <summary>
        /// Bass level of the zone.
        /// </summary>
        int Bass { get; set; }

        /// <summary>
        /// Treble level of the zone.
        /// </summary>
        int Treble { get; set; }

        /// <summary>
        /// Whether loudness is on or off.
        /// </summary>
        Power Loudness { get; set; }

        /// <summary>
        /// Balance level of the zone.
        /// </summary>
        int Balance { get; set; }

        /// <summary>
        /// Party mode of the zone.
        /// </summary>
        PartyMode PartyMode { get; }

        /// <summary>
        /// Do not disturb mode of the zone.
        /// </summary>
        DoNotDisturbMode DoNotDisturbMode { get; }

        /// <summary>
        /// Initiates a load of the values.
        /// </summary>
        /// <returns></returns>
        Task LoadAsync();

        /// <summary>
        /// Saves the modified values.
        /// </summary>
        /// <returns></returns>
        Task SaveAsync();

        Task SetVolume(int value);

    }

}
