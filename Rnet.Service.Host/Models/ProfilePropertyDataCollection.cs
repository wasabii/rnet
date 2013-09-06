using System.Collections.Generic;

using Newtonsoft.Json;

namespace Rnet.Service.Models
{

    [JsonConverter(typeof(ProfilePropertyDataCollectionJsonConverter))]
    public class ProfilePropertyDataCollection : List<ProfilePropertyData>
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
