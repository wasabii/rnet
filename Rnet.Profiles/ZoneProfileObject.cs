namespace Rnet.Profiles
{

    /// <summary>
    /// Serves as a simple profile implementation base for a device..
    /// </summary>
    public abstract class ZoneProfileObject : ProfileObject
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="zone"></param>
        protected ZoneProfileObject(RnetZone zone)
            : base(zone)
        {

        }

        /// <summary>
        /// The zone implementing this profile.
        /// </summary>
        protected RnetZone Zone
        {
            get { return (RnetZone)Target; }
        }

    }

}
