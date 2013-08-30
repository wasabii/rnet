using System;
using System.Runtime.Serialization;

namespace Rnet.Service.Devices
{

    [DataContract]
    class ControllerRef
    {

        [DataMember]
        public Uri Id { get; set; }

    }

}
