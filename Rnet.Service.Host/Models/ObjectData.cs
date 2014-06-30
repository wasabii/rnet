using System;
using System.Diagnostics.Contracts;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Rnet.Service.Host.Models
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
            set { Uri = value != null ? new Uri(value) : null; }
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
