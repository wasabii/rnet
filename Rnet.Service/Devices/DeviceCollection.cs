using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Rnet.Service.Devices
{

    [CollectionDataContract(Name = "Devices", Namespace = "urn:Rnet.Devices")]
    [KnownType(typeof(Controller))]
    [KnownType(typeof(Device))]
    class DeviceCollection : List<Device>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public DeviceCollection()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="devices"></param>
        public DeviceCollection(IEnumerable<Device> devices)
            : base(devices)
        {

        }

    }

}
