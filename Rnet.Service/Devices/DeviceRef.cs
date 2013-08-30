using System;
using System.Runtime.Serialization;

namespace Rnet.Service.Devices
{

    [DataContract]
    class DeviceRef
    {

        [DataMember]
        public Uri Id{ get; set; }

    }

}
