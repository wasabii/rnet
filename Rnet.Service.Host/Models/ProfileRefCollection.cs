using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Rnet.Service.Host.Models
{

    [XmlRoot(ElementName = "Profiles", Namespace = "urn:rnet:objects")]
    [JsonConverter(typeof(ProfileRefCollectionJsonConverter))]
    public class ProfileRefCollection : List<ProfileRef>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ProfileRefCollection()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="refs"></param>
        public ProfileRefCollection(IEnumerable<ProfileRef> refs)
            : base(refs)
        {

        }

    }

}
