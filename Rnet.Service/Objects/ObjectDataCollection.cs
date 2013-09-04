using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Rnet.Service.Objects
{

    [XmlRoot("Objects", Namespace = "urn:rnet")]
    [JsonConverter(typeof(ObjectDataCollectionJsonConverter))]
    public class ObjectDataCollection : List<ObjectData>
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
