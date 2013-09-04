using System;
using System.Xml.Serialization;

namespace Rnet.Service.Objects
{

    public class ProfileRef
    {

        [XmlIgnore]
        public Uri Uri { get; set; }

        [XmlAttribute("Uri")]
        public string _Uri
        {
            get { return Uri != null ? Uri.ToString() : null; }
            set { Uri = new Uri(value); }
        }

        [XmlAttribute("Id")]
        public string Id { get; set; }

    }

}
