using System;
using System.Runtime.Serialization;

namespace Rnet.Service.Devices
{

    [DataContract(Namespace = "urn:rnet:devices")]
    public class Device
    {

        [DataMember(Order = 0)]
        public Uri Id { get; set; }

        [DataMember(Order = 1)]
        public string DeviceId { get; set; }

    }

}
