using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Rnet.Service.Objects
{

    [DataContract(Namespace = "urn:rnet:objects", Name = "Profile")]
    [KnownType(typeof(ProfilePropertyData))]
    [JsonObject("Profile")]
    class ProfileData
    {

        [DataMember]
        public Uri Href { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public ProfilePropertyDataCollection Properties { get; set; }

    }

}
