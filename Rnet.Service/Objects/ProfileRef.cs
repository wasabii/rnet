using System;
using System.Runtime.Serialization;

namespace Rnet.Service.Objects
{

    [DataContract(Namespace = "urn:rnet:objects")]
    class ProfileRef
    {

        [DataMember]
        public Uri Href { get; set; }
        
        [DataMember]
        public string Id { get;set;}

    }

}
