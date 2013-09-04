using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Rnet.Service.Objects
{

    /// <summary>
    /// Base collection for <see cref="ObjectData"/>s.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ObjectDataCollection<T> : List<T>
        where T : ObjectData
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ObjectDataCollection()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="items"></param>
        public ObjectDataCollection(IEnumerable<T> items)
            : base(items)
        {

        }

    }

    [XmlRoot("Objects", Namespace = "urn:rnet")]
    [JsonConverter(typeof(ObjectDataCollectionJsonConverter))]
    public class ObjectDataCollection : ObjectDataCollection<ObjectData>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ObjectDataCollection()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="items"></param>
        public ObjectDataCollection(IEnumerable<ObjectData> items)
            : base(items)
        {

        }

    }

}
