using System.ServiceModel;

namespace Rnet.Profiles
{

    /// <summary>
    /// Basic information exposed by any RNET object.
    /// </summary>
    [ServiceContract(Namespace = "urn:rnet:profiles", Name = "object")]
    public interface IObject
    {

        /// <summary>
        /// Identifier of the object, in relation to it's container.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Simple display name of the object.
        /// </summary>
        string DisplayName { get; }

    }

}
