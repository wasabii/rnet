using System;
using System.Runtime.Serialization;

namespace Rnet.Service
{

    [DataContract]
    class BusZoneRef
    {

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public Uri Uri { get; set; }

    }

}
