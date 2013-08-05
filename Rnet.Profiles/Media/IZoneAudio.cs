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
        /// Volume level of the zone.
        /// </summary>
        /// <remarks>
        /// 0:100 = data 0:50
        /// </remarks>
        int Volume { get; set; }

        /// <summary>
        /// Turns the volume up.
        /// </summary>
        /// <returns></returns>
        void VolumeUp();

        /// <summary>
        /// Turns the volume down.
        /// </summary>
        /// <returns></returns>
        void VolumeDown();

        /// <summary>
        /// Bass level of the zone.
        /// </summary>
        /// <remarks>
        /// -10:+10 data 0:20
        /// </remarks>
        int Bass { get; set; }

        /// <summary>
        /// Turns the bass up.
        /// </summary>
        void BassUp();

        /// <summary>
        /// Turns the bass down.
        /// </summary>
        void BassDown();

        /// <summary>
        /// Treble level of the zone.
        /// </summary>
        /// <remarks>
        /// -10:+10 data 0:20
        /// </remarks>
        int Treble { get; set; }

        /// <summary>
        /// Turns the treble up.
        /// </summary>
        void TrebleUp();

        /// <summary>
        /// Turns the treble down.
        /// </summary>
        void TrebleDown();

        /// <summary>
        /// Whether loudness is on or off.
        /// </summary>
        Loudness Loudness { get; set; }

        /// <summary>
        /// Balance level of the zone.
        /// </summary>
        /// <remarks>
        /// -10:+10 data 0:20 (left to right)
        /// </remarks>
        int Balance { get; set; }

        /// <summary>
        /// Shifts the balance to the left.
        /// </summary>
        void BalanceLeft();

        /// <summary>
        /// Shifts the balance to the right.
        /// </summary>
        void BalanceRight();

        /// <summary>
        /// Party mode of the zone.
        /// </summary>
        PartyMode PartyMode { get; }

    }

}
