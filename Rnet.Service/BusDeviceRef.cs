using System;
using System.Runtime.Serialization;

namespace Rnet.Service
{

    [DataContract]
    class BusDeviceRef
    {

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string DeviceId { get; set; }

        [DataMember]
        public Uri Uri{ get; set; }

    }

}
