using System;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Rnet.Service.Objects
{

    [XmlSchemaProvider("GetSchema", IsAny = true)]
    public class ProfilePropertyData : IXmlSerializable
    {

        public Uri Uri { get; set; }

        public Uri FriendlyUri { get; set; }

        public string Name { get; set; }

        public object Value { get; set; }

        public string XmlNamespace { get; set; }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteXml(XmlWriter writer)
        {
            var ns = (XNamespace)XmlNamespace;

            new XElement(ns + Name,
                Uri != null ? new XAttribute("Uri", Uri) : null,
                Uri != null ? new XAttribute("FriendlyUri", FriendlyUri) : null,
                new XElement(ns + "Value",
                    Value)).WriteTo(writer);
        }

    }

}
