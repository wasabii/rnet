using System;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Rnet.Service.Objects
{

    [JsonObject("Property")]
    public class ProfilePropertyData
    {

        [XmlIgnore]
        [JsonProperty]
        public Uri Href { get; set; }

        [XmlAttribute("Href")]
        [JsonIgnore]
        public string _Href
        {
            get { return Href.ToString(); }
            set { Href = new Uri(value); }
        }

        [JsonIgnore]
        public string Name { get; set; }

        [JsonProperty]
        public object Value { get; set; }

    }

}
