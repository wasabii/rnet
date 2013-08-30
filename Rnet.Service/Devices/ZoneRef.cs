using System;
using System.Runtime.Serialization;

namespace Rnet.Service.Devices
{

    [DataContract]
    class ZoneRef
    {

        [DataMember]
        public Uri Id { get; set; }

    }

}
