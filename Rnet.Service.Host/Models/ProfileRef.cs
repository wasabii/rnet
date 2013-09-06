using System;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Rnet.Service.Models
{

    [JsonObject("ProfileRef")]
    public class ProfileRef
    {

        [XmlIgnore]
        public Uri Uri { get; set; }

        [XmlAttribute("Uri")]
        public string _Uri
        {
            get { return Uri != null ? Uri.ToString() : null; }
            set { Uri = new Uri(value); }
        }

        [XmlIgnore]
        public Uri FriendlyUri { get; set; }

        [XmlAttribute("FriendlyUri")]
        [JsonIgnore]
        public string _FriendlyUri
        {
            get { return FriendlyUri != null ? FriendlyUri.ToString() : null; }
            set { FriendlyUri = new Uri(value); }
        }

        [XmlAttribute]
        public string Id { get; set; }

    }

}
