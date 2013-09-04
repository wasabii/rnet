using System;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Rnet.Service.Objects
{

    [XmlRoot("")]
    [JsonObject("Profile")]
    public class ProfileData : IXmlSerializable
    {

        public Uri Uri { get; set; }

        [JsonIgnore]
        public string _Uri
        {
            get { return Uri != null ? Uri.ToString() : null; }
            set { Uri = new Uri(value); }
        }

        public string Id { get; set; }

        public ProfilePropertyDataCollection Properties { get; set; }

        public ProfileCommandDataCollection Commands { get; set; }

        public string Name { get; set; }

        public string Namespace { get; set; }

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
            ProfileToXml().WriteTo(writer);
        }

        XElement ProfileToXml()
        {
            var ns = (XNamespace)("urn:rnet:profiles:" + Id);

            var xml = new XElement(ns + Name,
                new XAttribute("Uri", Uri),
                new XAttribute("Id", Id),
                Properties.Select(i => PropertyToXml(i)));

            return xml;
        }

        XElement PropertyToXml(ProfilePropertyData property)
        {
            var ns = (XNamespace)("urn:rnet:profiles:" + Id);

            return new XElement(ns + property.Name,
                new XAttribute("Uri", property.Uri),
                new XElement(ns + "Value",
                    property.Value));
        }

    }

}
