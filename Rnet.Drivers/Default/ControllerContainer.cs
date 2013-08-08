using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

using Rnet.Profiles;

namespace Rnet.Drivers.Default
{

    /// <summary>
    /// Provides a default implementation of <see cref="IContainer"/> and <see cref="IOwner"/> for a
    /// controller. This implementation provides the addressable zone objects as its contents.
    /// </summary>
    public class ControllerContainer : ControllerBase, IContainer, IOwner
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        protected internal ControllerContainer(RnetController target)
            : base(target)
        {
            Controller.Zones.CollectionChanged += (s, a) => RaiseCollectionChanged(a);
        }

        public virtual IEnumerator<RnetBusObject> GetEnumerator()
        {
            return Controller.Zones.GetEnumerator();
        }

        /// <summary>
        /// Obtains the profiles for a nested bus object.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="profileType"></param>
        /// <returns></returns>
        public virtual Task<object[]> GetProfiles(RnetBusObject target)
        {
            var zone = target as RnetZone;
            if (zone == null)
                return null;

            // our zones only
            if (zone.Controller != Controller)
                return null;

            return Task.FromResult(new object[]
            { 
                new Zone(zone),
                new ZoneContainer(zone) 
            });
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
