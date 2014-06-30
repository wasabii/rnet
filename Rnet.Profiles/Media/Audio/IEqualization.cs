namespace Rnet.Profiles.Media.Audio
{

    /// <summary>
    /// Basic audio information for a zone.
    /// </summary>
    [ProfileContract("media.audio", "Equalization")]
    public interface IEqualization :
        Rnet.Profiles.Media.IZone
    {

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

    }

}
