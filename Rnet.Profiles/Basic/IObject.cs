namespace Rnet.Profiles.Basic
{

    /// <summary>
    /// Basic information exposed by any RNET object.
    /// </summary>
    public interface IObject : IProfile
    {

        /// <summary>
        /// Simple display name of the object.
        /// </summary>
        string Name { get; }

    }

}
