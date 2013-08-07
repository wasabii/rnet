namespace Rnet.Profiles.Basic
{

    public interface IZone : Driver
    {

        /// <summary>
        /// Gets or sets the user designated name for the zone.
        /// </summary>
        string Name { get; set; }

    }

}
