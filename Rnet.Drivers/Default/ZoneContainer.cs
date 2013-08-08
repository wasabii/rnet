using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

using Rnet.Profiles;

namespace Rnet.Drivers.Default
{

    /// <summary>
    /// Provides a default implementation of <see cref="IContainer"/> for a zone. This implementation provides the
    /// physical device objects as its contents, but does not own any.
    /// </summary>
    public class ZoneContainer : ZoneBase, IContainer
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="zone"></param>
        protected internal ZoneContainer(RnetZone zone)
            : base(zone)
        {
            Zone.Devices.CollectionChanged += (s, a) => RaiseCollectionChanged(a);
        }

        public virtual IEnumerator<RnetBusObject> GetEnumerator()
        {
            return Zone.Devices.GetEnumerator();
        }

        /// <summary>
        /// Raised when the set of nested bus objects is changed.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Raises the CollectionChanged event.
        /// </summary>
        /// <param name="args"></param>
        void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, args);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

}
