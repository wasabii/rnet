using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

using Rnet.Profiles;

namespace Rnet.Drivers.Default
{

    /// <summary>
    /// Provides a default implementation of <see cref="IContainer"/> and <see cref="IOwner"/> for a
    /// zone. This implementation provides the physical device objects as its contents.
    /// </summary>
    public class ZoneBusObjectContainer : ZoneProfileBase, IContainer, IOwner
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="zone"></param>
        protected internal ZoneBusObjectContainer(RnetZone zone)
            : base(zone)
        {

        }

        public IEnumerator<RnetBusObject> GetEnumerator()
        {
            return Zone.Devices.GetEnumerator();
        }

        /// <summary>
        /// Obtains the profiles for a nested bus object.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="profileType"></param>
        /// <returns></returns>
        public Task<object[]> GetProfiles(RnetBusObject target)
        {
            var zone = target as RnetZone;
            if (zone == null)
                return null;

            // our zones only
            if (zone.Controller != Zone.Controller)
                return null;

            return Task.FromResult<object[]>(new[] { new ZoneBusObjectContainer(zone) });
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
