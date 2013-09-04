using Newtonsoft.Json;

namespace Rnet.Service.Objects
{

    [JsonObject]
    public class ProfileData
    {

        [JsonProperty]
        public string Id { get; set; }

        [JsonProperty]
        public ProfilePropertyDataCollection Properties { get; set; }

        [JsonProperty]
        public ProfileCommandDataCollection Commands { get; set; }

    }

}
