using System;
using Newtonsoft.Json;

namespace Rnet.Service.Objects
{

    [JsonObject]
    class ProfileData
    {

        [JsonProperty]
        public string Id { get; set; }

        [JsonProperty]
        public ProfilePropertyDataCollection Properties { get; set; }

    }

}
