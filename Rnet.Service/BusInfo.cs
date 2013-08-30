using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Rnet.Service
{

    [DataContract]
    class BusInfo
    {

        [DataMember]
        public List<BusControllerRef> Controllers { get; set; }

    }

}
