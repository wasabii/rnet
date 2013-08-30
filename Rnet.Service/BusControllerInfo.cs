using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Rnet.Service
{

    [DataContract]
    class BusControllerInfo : BusDeviceInfo
    {

        [DataMember]
        public List<BusZoneRef> Zones { get; set; }

    }

}
