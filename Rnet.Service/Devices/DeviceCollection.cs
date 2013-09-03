using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Rnet.Service.Devices
{

    [XmlRoot("Devices", Namespace = "urn:rnet:devices")]
    public class DeviceCollection : List<Device>, IXmlSerializable
    {

        static readonly string ns = "urn:rnet:devices";

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public DeviceCollection()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="devices"></param>
        public DeviceCollection(IEnumerable<Device> devices)
            : base(devices)
        {

        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            throw new System.NotImplementedException();
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            foreach (var device in this)
                new XmlSerializer(device.GetType()).Serialize(writer, device);
        }

    }

}
