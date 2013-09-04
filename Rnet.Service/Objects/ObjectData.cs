using System;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Rnet.Service.Objects
{

    [XmlRoot("Object", Namespace = "urn:rnet")]
    public class ObjectData
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

        [XmlAttribute]
        public string Id { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlArray]
        [XmlArrayItem("Controller", typeof(ControllerData))]
        [XmlArrayItem("Device", typeof(DeviceData))]
        [XmlArrayItem("Object", typeof(ObjectData))]
        public ObjectDataCollection Objects { get; set; }

        [XmlArray]
        [XmlArrayItem("ProfileRef")]
        public ProfileRefCollection Profiles { get; set; }

    }

}
