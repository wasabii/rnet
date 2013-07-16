using System;
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
    public class RnetDataItemCollection : IEnumerable<RnetDataItem>, INotifyCollectionChanged
    {

        Dictionary<RnetPath, RnetDataItem> items =
            new Dictionary<RnetPath, RnetDataItem>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetDataItemCollection(RnetDevice device)
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
        internal void Add(RnetDataItem item)
        {
            lock (items)
            {
                var oldItem = items.ValueOrDefault(item.Path);
                items[item.Path] = item;

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
        internal void Remove(RnetDataItem item)
        {
            lock (items)
            {
                if (items.Remove(item.Path))
                    RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            }
        }

        /// <summary>
        /// Gets the <see cref="RnetDataItem"/> at the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<RnetDataItem> GetAsync(RnetPath path)
        {
            return GetAsync(path, CancellationToken.None);
        }

        /// <summary>
        /// Gets the <see cref="RnetDataItem"/> at the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<RnetDataItem> GetAsync(RnetPath path, CancellationToken cancellationToken)
        {
            lock (items)
            {
                var item = items.ValueOrDefault(path);
                if (item != null)
                    return Task.FromResult(item);

                return RequestItem(path, cancellationToken);
            }
        }

        /// <summary>
        /// Initiates a data request.
        /// </summary>
        /// <param name="path"></param>
        async Task<RnetDataItem> RequestItem(RnetPath path, CancellationToken cancellationToken)
        {
            var d = await Device.RequestDataItem(path, cancellationToken);
            if (d == null)
                return null;

            // add to collection
            Add(d);

            return d;
        }

        /// <summary>
        /// Gets the data at the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public RnetDataItem this[RnetPath path]
        {
            get { return items.ValueOrDefault(path); }
        }

        public IEnumerator<RnetDataItem> GetEnumerator()
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
