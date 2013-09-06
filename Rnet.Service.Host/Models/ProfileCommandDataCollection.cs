using System.Collections.Generic;

using Newtonsoft.Json;

namespace Rnet.Service.Models
{

    [JsonConverter(typeof(ProfileCommandDataCollectionJsonConverter))]
    public class ProfileCommandDataCollection : List<ProfileCommandData>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ProfileCommandDataCollection()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="data"></param>
        public ProfileCommandDataCollection(IEnumerable<ProfileCommandData> data)
            : base(data)
        {

        }

    }

}
