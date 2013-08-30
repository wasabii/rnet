using System;
using System.Runtime.Serialization;

namespace Rnet.Service.Objects
{

    [DataContract(Namespace = "urn:Rnet.Objects")]
    class Object
    {

        [DataMember]
        public Uri Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public ObjectRefCollection Objects { get; set; }

        [DataMember]
        public ProfileCollection Profiles { get; set; }

    }

}
