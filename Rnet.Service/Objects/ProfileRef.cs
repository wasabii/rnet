using System;
using System.Runtime.Serialization;

namespace Rnet.Service
{

    [DataContract]
    class ProfileRef
    {

        [DataMember]
        public Uri Uri { get; set; }

    }

}
