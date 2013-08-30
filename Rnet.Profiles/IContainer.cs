using System.Collections.Generic;
using System.Collections.Specialized;

namespace Rnet.Profiles
{

    /// <summary>
    /// Provided by <see cref="RnetBusObject"/>s that contain other nested <see cref="RnetBusObject"/>s.
    /// </summary>
    [Contract("urn:rnet:profiles", "Container")]
    public interface IContainer : IEnumerable<RnetBusObject>, INotifyCollectionChanged
    {



    }

}
