using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Rnet.Service.Objects
{

    [CollectionDataContract(Name = "Properties", Namespace = "urn:rnet:objects", ItemName = "Property")]
    [JsonConverter(typeof(ProfilePropertyDataCollectionJsonConverter))]
    class ProfilePropertyDataCollection : List<ProfilePropertyData>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ProfilePropertyDataCollection()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="data"></param>
        public ProfilePropertyDataCollection(IEnumerable<ProfilePropertyData> data)
            : base(data)
        {

        }

    }

}
