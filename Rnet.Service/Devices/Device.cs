using System;
using System.Runtime.Serialization;

namespace Rnet.Service.Devices
{

    [DataContract]
    class Device
    {

        [DataMember]
        public Uri Id { get; set; }

        [DataMember]
        public string DeviceId { get; set; }

        [DataMember]
        public ControllerRef Controller { get; set; }

        [DataMember]
        public ZoneRef Zone { get; set; }

    }

}
