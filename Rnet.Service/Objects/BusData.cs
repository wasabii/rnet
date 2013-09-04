using System.Xml.Serialization;

namespace Rnet.Service.Objects
{

    [XmlRoot("Bus", Namespace = "urn:rnet")]
    public class BusData
    {

        public DeviceDataCollection Devices { get; set; }

        public ObjectDataCollection Objects { get; set; }

    }

}
