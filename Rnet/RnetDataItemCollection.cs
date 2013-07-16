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

        AsyncCollection<RnetDataItem> items = new AsyncCollection<RnetDataItem>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetDataItemCollection()
        {
            items.CollectionChanged += (s, a) => RaiseCollectionChanged(a);
            items.SubscriberAdded += items_SubscriberAdded;
        }

        /// <summary>
        /// Invoked when a new subscriber is added.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void items_SubscriberAdded(object sender, AsyncCollectionSubscriberEventArgs<RnetDataItem> args)
        {
            RaiseRequestData((RnetPath)args.Subscriber.UserState);
        }

        /// <summary>
        /// Removes the given item.
        /// </summary>
        /// <param name="item"></param>
        internal void Remove(RnetDataItem item)
        {
            items.Remove(item);
        }

        /// <summary>
        /// Gets the <see cref="RnetDataItem"/> at the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<RnetDataItem> GetAsync(RnetPath path)
        {
            return items.GetAsync(i => i.Path == path, path);
        }

        /// <summary>
        /// Gets the <see cref="RnetDataItem"/> at the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<RnetDataItem> GetAsync(RnetPath path, CancellationToken cancellationToken)
        {
            return items.GetAsync(i => i.Path == path, cancellationToken, path);
        }

        /// <summary>
        /// Gets the data item at the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        RnetDataItem GetData(RnetPath path)
        {
            return items.FirstOrDefault(i => i.Path == path);
        }

        /// <summary>
        /// Gets the data at the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public RnetDataItem this[RnetPath path]
        {
            get { return GetData(path); }
        }

        /// <summary>
        /// Begins a write of new data to the specified path.
        /// </summary>
        /// <param name="path"></param>
        public void WriteBegin(RnetPath path, int packetCount)
        {
            var item = GetData(path);
            if (item == null)
                items.Add(item = new RnetDataItem(path));

            item.WriteBegin(packetCount);
        }

        /// <summary>
        /// Appends the data to the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        public void Write(RnetPath path, byte[] buffer, int packetNumber)
        {
            var item = GetData(path);
            if (item == null)
                throw new NullReferenceException();

            item.Write(buffer, packetNumber);
        }

        /// <summary>
        /// Finalizes writing to the path and makes the data available.
        /// </summary>
        /// <param name="path"></param>
        public void WriteEnd(RnetPath path)
        {
            var item = GetData(path);
            if (item == null)
                throw new NullReferenceException();

            item.WriteEnd();
        }

        /// <summary>
        /// Removes the data item at the given path.
        /// </summary>
        /// <param name="path"></param>
        public void Remove(RnetPath path)
        {
            var item = GetData(path);
            if (item != null)
            {
                items.Remove(item);
                item.Dispose();
            }
        }

        /// <summary>
        /// Raised when data is requested.
        /// </summary>
        internal event EventHandler<ValueEventArgs<RnetPath>> RequestData;

        /// <summary>
        /// Raises the SubscriberAdded event.
        /// </summary>
        /// <param name="path"></param>
        void RaiseRequestData(RnetPath path)
        {
            if (RequestData != null)
                RequestData(this, new ValueEventArgs<RnetPath>(path));
        }

        public IEnumerator<RnetDataItem> GetEnumerator()
        {
            return items.GetEnumerator();
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
