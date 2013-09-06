using System;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Rnet.Service.Models
{

    [XmlSchemaProvider("GetSchema", IsAny = true)]
    public class ProfilePropertyData : IXmlSerializable
    {

        public Uri Uri { get; set; }

        public Uri FriendlyUri { get; set; }

        public string Name { get; set; }

        public object Value { get; set; }

        [JsonProperty("_xmlns", Order = -10)]
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
                Uri != null ? new XAttribute("FriendlyUri", FriendlyUri) : null,
                new XElement(ns + "Value",
                    Value)).WriteTo(writer);
        }

    }

}
