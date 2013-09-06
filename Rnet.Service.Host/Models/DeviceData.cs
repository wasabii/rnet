using System;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Rnet.Service.Host.Models
{

    [XmlRoot("Device", Namespace = "urn:rnet")]
    public class DeviceData : ObjectData
    {

        [XmlAttribute]
        public string RnetId { get; set; }

        [XmlIgnore]
        [JsonProperty]
        public Uri DataUri { get; set; }

        [XmlAttribute("DataUri")]
        [JsonIgnore]
        public string _DataUri
        {
            get { return DataUri != null ? DataUri.ToString() : null; }
            set { DataUri = new Uri(value); }
        }

    }

}
