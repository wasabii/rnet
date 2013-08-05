namespace Rnet.Profiles.Basic
{

    public interface IZone : IProfile
    {

        /// <summary>
        /// Gets or sets the user designated name for the zone.
        /// </summary>
        new string Name { get; set; }

    }

}
