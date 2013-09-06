using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Rnet.Service.Host.Models
{

    [XmlRoot("Devices", Namespace = "urn:rnet")]
    [JsonConverter(typeof(DeviceDataCollectionJsonConverter))]
    public class DeviceDataCollection : List<DeviceData>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public DeviceDataCollection()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="items"></param>
        public DeviceDataCollection(IEnumerable<DeviceData> items)
            : base(items)
        {

        }

    }

}
