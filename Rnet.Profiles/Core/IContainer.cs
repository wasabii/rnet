using System.Collections.Generic;
using System.Collections.Specialized;

namespace Rnet.Profiles.Core
{

    /// <summary>
    /// Provided by <see cref="RnetBusObject"/>s that contain other nested <see cref="RnetBusObject"/>s.
    /// </summary>
    [ProfileContract("core", "Container")]
    public interface IContainer : IEnumerable<RnetBusObject>, INotifyCollectionChanged
    {



    }

}
