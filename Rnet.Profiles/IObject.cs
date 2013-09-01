using System.Xml.Serialization;

namespace Rnet.Profiles
{

    /// <summary>
    /// Basic information exposed by any RNET object.
    /// </summary>
    [ProfileContract("Object")]
    [XmlRoot(Namespace = "urn:rnet:profiles::Object", ElementName = "Object")]
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
