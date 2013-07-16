using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;

namespace Rnet
{

    /// <summary>
    /// Provides a collection of devices that supports waiting on a device.
    /// </summary>
    public class RnetDeviceCollection : IEnumerable<RnetDevice>, INotifyCollectionChanged
    {

        Dictionary<RnetDeviceId, RnetDevice> items =
            new Dictionary<RnetDeviceId, RnetDevice>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetDeviceCollection(RnetBus bus)
        {
            Bus = bus;
        }

        /// <summary>
        /// Bus devices are a member of.
        /// </summary>
        public RnetBus Bus { get; private set; }

        /// <summary>
        /// Adds the given device to the collection.
        /// </summary>
        /// <param name="device"></param>
        internal void Add(RnetDevice device)
        {
            lock (items)
            {
                var oldDevice = items.ValueOrDefault(device.Id);
                items[device.Id] = device;

                if (oldDevice == null)
                    RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, device));
                else if (!object.ReferenceEquals(oldDevice, device))
                    RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, device, oldDevice));
            }
        }

        /// <summary>
        /// Removes the given device from the collection.
        /// </summary>
        /// <param name="device"></param>
        internal void Remove(RnetDevice device)
        {
            lock (items)
            {
                if (!items.ContainsKey(device.Id))
                    return;

                // find existing index of device item
                var index = items.Values
                    .Select((i, j) => new { Index = j, Device = i })
                    .Where(i => i.Device.Id == device.Id)
                    .Select(i => i.Index)
                    .First();

                if (items.Remove(device.Id))
                    RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, device, index));
            }
        }

        /// <summary>
        /// Gets the specified device if it exists.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal RnetDevice this[RnetDeviceId id]
        {
            get
            {
                lock (items)
                    return items.ValueOrDefault(id);
            }
        }

        /// <summary>
        /// Gets the device given by the specified <see cref="RnetDeviceId"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<RnetDevice> GetAsync(RnetDeviceId id)
        {
            return GetAsync(id, RnetBus.GetDefaultCancellationToken());
        }

        /// <summary>
        /// Gets the device given by the specified <see cref="RnetDeviceId"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<RnetDevice> GetAsync(RnetDeviceId id, CancellationToken cancellationToken)
        {
            lock (items)
            {
                var device = items.ValueOrDefault(id);
                if (device != null)
                    return Task.FromResult(device);
            }

            return RequestDevice(id, cancellationToken);
        }

        /// <summary>
        /// Initiates a device request.
        /// </summary>
        /// <param name="deviceId"></param>
        async Task<RnetDevice> RequestDevice(RnetDeviceId deviceId, CancellationToken cancellationToken)
        {
            var d = await Bus.RequestDevice(deviceId, cancellationToken);
            if (d == null)
                return null;

            // add to collection
            Add(d);

            return d;
        }

        public IEnumerator<RnetDevice> GetEnumerator()
        {
            return items.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Raised when an device is added or removed.
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

    }

}
