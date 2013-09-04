using System;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Rnet.Service.Objects
{

    [XmlRoot("Device", Namespace = "urn:rnet")]
    [XmlInclude(typeof(ControllerData))]
    public class DeviceData : ObjectData
    {

        [XmlIgnore]
        [JsonProperty]
        public Uri DeviceHref { get; set; }

        [XmlAttribute("DeviceHref")]
        [JsonIgnore]
        public string _DeviceHref
        {
            get { return DeviceHref.ToString(); }
            set { DeviceHref = new Uri(value); }
        }

        [XmlAttribute("DeviceId")]
        public string DeviceId { get; set; }

    }

}
