using System.Xml.Serialization;

namespace Rnet.Service.Objects
{

    [XmlRoot("Object")]
    public class ObjectData
    {

        [XmlAttribute]
        public string Id { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        public ObjectRefCollection Objects { get; set; }

        public ProfileRefCollection Profiles { get; set; }

    }

}
