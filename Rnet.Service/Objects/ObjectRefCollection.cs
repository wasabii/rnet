using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Rnet.Service.Objects
{

    [XmlRoot("Objects", Namespace = "urn:rnet:objects")]
    [JsonConverter(typeof(ObjectRefCollectionJsonConverter))]
    public class ObjectRefCollection : List<ObjectRef>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ObjectRefCollection()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="refs"></param>
        public ObjectRefCollection(IEnumerable<ObjectRef> refs)
            : base(refs)
        {

        }

    }

}
