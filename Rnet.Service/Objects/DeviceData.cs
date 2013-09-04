using System;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Rnet.Service.Objects
{

    [XmlRoot("Device", Namespace = "urn:rnet")]
    public class DeviceData : ObjectData
    {

        [XmlAttribute]
        public string RnetId { get; set; }

    }

}
