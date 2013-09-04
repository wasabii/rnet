using System;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Rnet.Service.Objects
{

    [XmlRoot("Command")]
    [JsonObject("Command")]
    public class ProfileCommandData
    {

        [XmlIgnore]
        [JsonProperty]
        public Uri Uri { get; set; }

        [XmlAttribute("Uri")]
        [JsonIgnore]
        public string _Uri
        {
            get { return Uri != null ? Uri.ToString() : null; }
            set { Uri = new Uri(value); }
        }

        [JsonProperty]
        public string Name { get; set; }

    }

}
