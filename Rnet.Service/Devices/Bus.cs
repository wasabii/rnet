using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Rnet.Service.Devices
{

    [DataContract]
    class Bus
    {

        [DataMember]
        public List<ControllerRef> Controllers { get; set; }

    }

}
