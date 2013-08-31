namespace Rnet.Profiles
{

    /// <summary>
    /// Basic information exposed by any RNET object.
    /// </summary>
    [Contract("urn:rnet:profiles", "Object")]
    public interface IObject
    {

        /// <summary>
        /// Identifier of the object, in relation to it's container.
        /// </summary>
        [Value("Id")]
        string Id { get; }

        /// <summary>
        /// Simple display name of the object.
        /// </summary>
        [Value("DisplayName")]
        string DisplayName { get; }

    }

}
