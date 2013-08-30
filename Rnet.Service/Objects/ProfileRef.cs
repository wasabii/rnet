using System;
using System.Runtime.Serialization;

namespace Rnet.Service.Objects
{

    [DataContract(Namespace = "urn:rnet:objects")]
    class ProfileRef
    {

        [DataMember]
        public Uri Id { get; set; }
        
        [DataMember]
        public string Name { get;set;}

    }

}
