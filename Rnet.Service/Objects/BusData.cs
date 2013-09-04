using System.Xml.Serialization;

namespace Rnet.Service.Objects
{

    [XmlRoot("Bus", Namespace = "urn:rnet")]
    public class BusData
    {

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
