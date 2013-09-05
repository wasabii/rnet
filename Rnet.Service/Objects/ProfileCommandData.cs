using System;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Rnet.Service.Objects
{

    [XmlSchemaProvider("GetSchema", IsAny = true)]
    [JsonObject("Command")]
    public class ProfileCommandData : IXmlSerializable
    {

        public Uri Uri { get; set; }

        public Uri FriendlyUri { get; set; }

        public string Name { get; set; }

        public string XmlNamespace { get; set; }

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
            var ns = (XNamespace)XmlNamespace;

            new XElement(ns + Name,
                Uri != null ? new XAttribute("Uri", Uri) : null,
                FriendlyUri != null ? new XAttribute("FriendlyUri", FriendlyUri) : null)
                .WriteTo(writer);
        }

    }

}
