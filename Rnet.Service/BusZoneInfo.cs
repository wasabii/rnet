using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Rnet.Service
{

    [DataContract]
    class BusZoneInfo
    {

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public Uri Uri { get; set; }

        [DataMember]
        public BusControllerRef Controller { get; set; }

        [DataMember]
        public List<BusDeviceRef> Devices { get; set; }

    }

}
