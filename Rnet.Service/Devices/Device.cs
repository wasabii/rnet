using System;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Rnet.Service.Devices
{

    [XmlRoot("Device", Namespace = "urn:rnet:devices")]
    [XmlInclude(typeof(Controller))]
    public class Device
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

        [XmlAttribute("Id")]
        public string Id { get; set; }

    }

}
