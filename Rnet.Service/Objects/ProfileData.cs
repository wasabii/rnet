using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Rnet.Service.Objects
{

    [JsonObject("Profile")]
    [XmlSchemaProvider("GetSchema", IsAny = true)]
    public class ProfileData : IXmlSerializable
    {

        public static XmlQualifiedName GetSchema(XmlSchemaSet xs)
        {
            return new XmlQualifiedName();
        }

        public Uri Uri { get; set; }

        public Uri FriendlyUri { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public string Namespace { get; set; }

        [JsonProperty("_xmlns", Order = -10)]
        public string XmlNamespace { get; set; }

        public ProfilePropertyDataCollection Properties { get; set; }

        public ProfileCommandDataCollection Commands { get; set; }

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
            ToXml().WriteTo(writer);
        }

        XElement ToXml()
        {
            var ns = (XNamespace)XmlNamespace;

            return new XElement(ns + Name,
                new XAttribute("Uri", Uri),
                new XAttribute("FriendlyUri", FriendlyUri),
                new XAttribute("Id", Id),
                Properties.Select(i => i.ToXElement()),
                Commands.Select(i => i.ToXElement()));
        }

    }

}
