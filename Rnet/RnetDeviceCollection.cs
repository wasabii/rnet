using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;

namespace Rnet
{

    /// <summary>
    /// Provides a collection of devices that supports waiting on a device.
    /// </summary>
    public class RnetDeviceCollection : IEnumerable<RnetDevice>, INotifyCollectionChanged
    {

        AsyncCollection<RnetDevice> items = new AsyncCollection<RnetDevice>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetDeviceCollection()
        {
            items.CollectionChanged += (s, a) => RaiseCollectionChanged(a);
            items.SubscriberAdded += items_SubscriberAdded;
        }

        /// <summary>
        /// Invoked when a new subscriber is added.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void items_SubscriberAdded(object sender, AsyncCollectionSubscriberEventArgs<RnetDevice> args)
        {
            RaiseRequestDevice((RnetDeviceId)args.Subscriber.UserState);
        }

        /// <summary>
        /// Adds the given device to the collection.
        /// </summary>
        /// <param name="device"></param>
        internal void Add(RnetDevice device)
        {
            items.Add(device);
        }

        /// <summary>
        /// Removes the given device from the collection.
        /// </summary>
        /// <param name="device"></param>
        internal void Remove(RnetDevice device)
        {
            items.Remove(device);
        }

        /// <summary>
        /// Gets the device given by the specified <see cref="RnetDeviceId"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<RnetDevice> GetAsync(RnetDeviceId id)
        {
            return items.GetAsync(i => i.Id == id, id);
        }

        /// <summary>
        /// Gets the device given by the specified <see cref="RnetDeviceId"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<RnetDevice> GetAsync(RnetDeviceId id, CancellationToken cancellationToken)
        {
            return items.GetAsync(i => i.Id == id, cancellationToken, id);
        }

        /// <summary>
        /// Raised when a device is requested.
        /// </summary>
        internal event EventHandler<ValueEventArgs<RnetDeviceId>> RequestDevice;

        /// <summary>
        /// Raises the RequestDevice event.
        /// </summary>
        /// <param name="id"></param>
        void RaiseRequestDevice(RnetDeviceId id)
        {
            if (RequestDevice != null)
                RequestDevice(this, new ValueEventArgs<RnetDeviceId>(id));
        }

        public IEnumerator<RnetDevice> GetEnumerator()
        {
            return items.GetEnumerator();
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
