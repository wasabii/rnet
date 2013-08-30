using System;
using System.Runtime.Serialization;

namespace Rnet.Service
{

    [DataContract]
    class ProfileInfo
    {

        [DataMember]
        public Uri Uri { get; set; }

    }

}
