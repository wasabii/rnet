using System.Collections.Generic;
using System.Collections.Specialized;

namespace Rnet.Drivers.Profiles
{

    /// <summary>
    /// Provided by <see cref="RnetBusObject"/>s that contain other nested <see cref="RnetBusObjects"/>.
    /// </summary>
    public interface IBusObjectContainer : IEnumerable<RnetBusObject>, INotifyCollectionChanged
    {

        /// <summary>
        /// The <see cref="RnetBusObject"/> which is the container of these objects.
        /// </summary>
        RnetBusObject Object { get; }

    }

}
