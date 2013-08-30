using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Rnet.Service.Devices
{

    [DataContract]
    class Controller
    {

        [DataMember]
        public Uri Id { get; set; }

        [DataMember]
        public string DeviceId { get; set; }

        [DataMember]
        public List<ZoneRef> Zones { get; set; }

    }

}
