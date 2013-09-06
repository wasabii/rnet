using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Rnet.Service.Models
{

    [XmlRoot("Data", Namespace = "urn:rnet")]
    public class DataHandleCollection : List<DataHandleData>, IXmlSerializable
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public DataHandleCollection()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="items"></param>
        public DataHandleCollection(IEnumerable<DataHandleData> items)
            : base(items)
        {

        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            var srs = new XmlSerializer(typeof(DataHandleData));
            foreach (var item in this)
                srs.Serialize(writer, item);
        }

    }

}
