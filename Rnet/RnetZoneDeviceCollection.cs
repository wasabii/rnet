using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Rnet
{

    /// <summary>
    /// Manages a collection of devices assigned to a zone.
    /// </summary>
    public sealed class RnetZoneDeviceCollection : IEnumerable<RnetDevice>, INotifyCollectionChanged
    {

        ConcurrentDictionary<RnetKeypadId, WeakReference<RnetDevice>> devices =
            new ConcurrentDictionary<RnetKeypadId, WeakReference<RnetDevice>>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="zone"></param>
        internal RnetZoneDeviceCollection(RnetZone zone)
        {
            Zone = zone;
        }

        /// <summary>
        /// The zone for which these devices are underneath.
        /// </summary>
        RnetZone Zone { get; set; }

        /// <summary>
        /// Gets a device with the given identifier.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RnetDevice this[RnetKeypadId id]
        {
            get { return GetOrCreate(id); }
            internal set { devices.GetOrAdd(id, new WeakReference<RnetDevice>(value)); }
        }

        /// <summary>
        /// Gets or creates a new device object for the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        RnetDevice GetOrCreate(RnetKeypadId id)
        {
            // skip reserved ids except external devices
            if (id != RnetKeypadId.External &&
                id != RnetKeypadId.External2 &&
                RnetKeypadId.IsReserved(id))
                return null;

            return devices
                .GetOrCreate(id, i => new RnetZoneRemoteDevice(Zone, id));
        }

        /// <summary>
        /// Returns an enumerator that iterates through the known devices.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<RnetDevice> GetEnumerator()
        {
            return devices.Values
                .Select(i => i.GetTargetOrDefault())
                .Where(i => i != null)
                .Where(i => i.IsActive)
                .OrderBy(i => i.DeviceId)
                .ToList()
                .GetEnumerator();
        }

        /// <summary>
        /// Invoked when a device becomes active.
        /// </summary>
        /// <param name="device"></param>
        internal void OnDeviceActive(RnetDevice device)
        {
            Zone.Activate();
            RaiseCollectionChanged();
        }

        /// <summary>
        /// Raised when devices are added or removed.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Raises the CollectionChanged event.
        /// </summary>
        void RaiseCollectionChanged()
        {
            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

}
