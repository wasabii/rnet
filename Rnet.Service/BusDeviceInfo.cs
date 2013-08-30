using System;
using System.Runtime.Serialization;

namespace Rnet.Service
{

    [DataContract]
    class BusDeviceInfo
    {

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string DeviceId { get; set; }

        [DataMember]
        public BusControllerRef Controller { get; set; }

        [DataMember]
        public BusZoneRef Zone { get; set; }

        [DataMember]
        public Uri Uri { get; set; }

    }

}
