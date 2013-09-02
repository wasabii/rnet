using System;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Rnet.Service.Objects
{

    [DataContract(Namespace = "urn:rnet:objects", Name = "Property")]
    [JsonObject("Property")]
    class ProfilePropertyData
    {

        [DataMember]
        public Uri Href { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public object Value { get; set; }

    }

}
