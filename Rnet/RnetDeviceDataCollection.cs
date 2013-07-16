using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rnet
{

    /// <summary>
    /// Stores a set of <see cref="DataItem"/>s.
    /// </summary>
    public class RnetDeviceDataCollection : IEnumerable<RnetDeviceData>, INotifyCollectionChanged
    {

        Dictionary<RnetPath, RnetDeviceData> items =
            new Dictionary<RnetPath, RnetDeviceData>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetDeviceDataCollection(RnetDevice device)
        {
            Device = device;
        }

        /// <summary>
        /// Device these data items are a member of.
        /// </summary>
        public RnetDevice Device { get; private set; }

        /// <summary>
        /// Adds the given item to the collection.
        /// </summary>
        /// <param name="item"></param>
        void Add(RnetDeviceData item)
        {
            lock (items)
            {
                // replace old item with new
                var oldItem = items.ValueOrDefault(item.Path);
                items[item.Path] = item;

                // raise appropriate event
                if (oldItem == null)
                    RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
                else if (!object.ReferenceEquals(oldItem, item))
                    RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem));
            }
        }

        /// <summary>
        /// Removes the given item from the collection.
        /// </summary>
        /// <param name="item"></param>
        void Remove(RnetDeviceData item)
        {
            lock (items)
            {
                // item already removed
                if (!items.ContainsKey(item.Path))
                    return;

                // find existing index of device item
                var index = items.Values
                    .Select((i, j) => new { Index = j, DataItem = i })
                    .Where(i => i.DataItem.Path == item.Path)
                    .Select(i => i.Index)
                    .First();

                if (items.Remove(item.Path))
                    RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            }
        }

        /// <summary>
        /// Gets the cached data at the specified path if already retrieved.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public RnetDeviceData this[RnetPath path]
        {
            get
            {
                lock (items)
                    return items.ValueOrDefault(path);
            }
        }

        /// <summary>
        /// Gets the <see cref="RnetDeviceData"/> at the specified path, returning from cache or the device as appropriate.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<RnetDeviceData> GetAsync(RnetPath path)
        {
            return GetAsync(path, RnetBus.CreateDefaultCancellationToken());
        }

        /// <summary>
        /// Gets the <see cref="RnetDeviceData"/> at the specified path, returning from cache or the device as appropriate.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<RnetDeviceData> GetAsync(RnetPath path, CancellationToken cancellationToken)
        {
            lock (items)
            {
                // check cache
                var item = items.ValueOrDefault(path);
                if (item != null)
                    return Task.FromResult(item);
            }

            return RequestDataAsync(path, cancellationToken);
        }

        /// <summary>
        /// Requests the data for the specified path from the device.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<RnetDeviceData> RequestDataAsync(RnetPath path, CancellationToken cancellationToken)
        {
            // request data from device
            var d = await Device.RequestDataAsync(path, cancellationToken);
            if (d == null)
                return null;

            // add to collection
            Add(d);

            return d;
        }

        /// <summary>
        /// Returns an enumerator that iterates through all the available data items.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<RnetDeviceData> GetEnumerator()
        {
            return items.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Raised when a data item is added or removed.
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
