using System;
using System.Diagnostics.Contracts;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Rnet.Service.Host.Models
{

    [XmlRoot("Item", Namespace = "urn:rnet")]
    public class DataHandleData
    {

        [XmlIgnore]
        [JsonProperty]
        public Uri Uri { get; set; }

        [XmlAttribute("Uri")]
        [JsonIgnore]
        public string _Uri
        {
            get { return Uri != null ? Uri.ToString() : null; }
            set { Contract.Requires<ArgumentNullException>(value != null); Uri = new Uri(value); }
        }

        [XmlIgnore]
        [JsonProperty]
        public Uri FriendlyUri { get; set; }

        [XmlAttribute("FriendlyUri")]
        [JsonIgnore]
        public string _FriendlyUri
        {
            get { return FriendlyUri != null ? FriendlyUri.ToString() : null; }
            set { Contract.Requires<ArgumentNullException>(value != null); FriendlyUri = new Uri(value); }
        }

        [XmlAttribute]
        public string Path { get; set; }

    }

}
