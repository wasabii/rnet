using Newtonsoft.Json;

namespace Rnet.Service.Host.Models
{

    [JsonConverter(typeof(ProfileCommandRequestJsonConverter))]
    public class ProfileCommandRequest
    {

        /// <summary>
        /// Gets or sets the parameters to be executed.
        /// </summary>
        public ProfileCommandParameter[] Parameters { get; set; }

    }

}
