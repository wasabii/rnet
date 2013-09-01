using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml.Serialization;

namespace Rnet.Profiles
{

    /// <summary>
    /// Provided by <see cref="RnetBusObject"/>s that contain other nested <see cref="RnetBusObject"/>s.
    /// </summary>
    [ProfileContract("Container")]
    [XmlRoot(Namespace = "urn:rnet:profiles::Container", ElementName = "Container")]
    public interface IContainer : IEnumerable<RnetBusObject>, INotifyCollectionChanged
    {



    }

}
