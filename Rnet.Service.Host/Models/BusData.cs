using System;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Rnet.Service.Models
{

    [XmlRoot("Bus", Namespace = "urn:rnet")]
    public class BusData
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

        [XmlArray]
        [XmlArrayItem("Controller", typeof(ControllerData))]
        [XmlArrayItem("Device", typeof(DeviceData))]
        public DeviceDataCollection Devices { get; set; }

        [XmlArray]
        [XmlArrayItem("Controller", typeof(ControllerData))]
        [XmlArrayItem("Device", typeof(DeviceData))]
        [XmlArrayItem("Object", typeof(ObjectData))]
        public ObjectDataCollection Objects { get; set; }

    }

}
