using System.Collections.Generic;
using System.Collections.Specialized;
using System.ServiceModel;

namespace Rnet.Profiles
{

    /// <summary>
    /// Provided by <see cref="RnetBusObject"/>s that contain other nested <see cref="RnetBusObject"/>s.
    /// </summary>
    [ServiceContract(Name = "container")]
    public interface IContainer : IEnumerable<RnetBusObject>, INotifyCollectionChanged
    {



    }

}
