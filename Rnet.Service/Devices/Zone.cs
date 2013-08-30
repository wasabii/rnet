using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Rnet.Service.Devices
{

    [DataContract]
    class Zone
    {

        [DataMember]
        public Uri Id { get; set; }

        [DataMember]
        public ControllerRef Controller { get; set; }

        [DataMember]
        public List<DeviceRef> Devices { get; set; }

    }

}
