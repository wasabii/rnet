using System;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Rnet.Service.Objects
{

    [XmlRoot("Object", Namespace = "urn:rnet")]
    [XmlInclude(typeof(DeviceData))]
    [XmlInclude(typeof(ControllerData))]
    public class ObjectData
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

        [XmlAttribute]
        public string Id { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        public ObjectDataCollection Objects { get; set; }

        public ProfileRefCollection Profiles { get; set; }

    }

}
