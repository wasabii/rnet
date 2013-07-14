using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rnet
{

    /// <summary>
    /// Provides a collection of devices that supports waiting on a device.
    /// </summary>
    public class RnetDeviceCollection : IEnumerable<RnetDevice>
    {

        AsyncCollection<RnetDevice> items = new AsyncCollection<RnetDevice>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetDeviceCollection()
        {
            items.SubscriberAdded += items_SubscriberAdded;
        }

        /// <summary>
        /// Invoked when a new subscriber is added.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void items_SubscriberAdded(object sender, AsyncCollectionSubscriberEventArgs<RnetDevice> args)
        {
            RaiseSubscriberAdded((RnetDeviceId)args.Subscriber.UserState);
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
        /// Raised when a subscriber for a device is added.
        /// </summary>
        internal event EventHandler<ValueEventArgs<RnetDeviceId>> SubscriberAdded;

        /// <summary>
        /// Raises the SubscriberAdded event.
        /// </summary>
        /// <param name="id"></param>
        void RaiseSubscriberAdded(RnetDeviceId id)
        {
            if (SubscriberAdded != null)
                SubscriberAdded(this, new ValueEventArgs<RnetDeviceId>(id));
        }

        public IEnumerator<RnetDevice> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

}
