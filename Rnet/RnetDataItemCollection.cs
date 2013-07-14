using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnet
{

    /// <summary>
    /// Stores a set of <see cref="DataItem"/>s.
    /// </summary>
    public class RnetDataItemCollection : IEnumerable<RnetDataItem>
    {

        AsyncCollection<RnetDataItem> items = new AsyncCollection<RnetDataItem>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetDataItemCollection()
        {
            items.SubscriberAdded += items_SubscriberAdded;
        }

        /// <summary>
        /// Invoked when a new subscriber is added.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void items_SubscriberAdded(object sender, AsyncCollectionSubscriberEventArgs<RnetDataItem> args)
        {
            RaiseSubscriberAdded((RnetPath)args.Subscriber.UserState);
        }

        internal void Remove(RnetDataItem device)
        {
            items.Remove(device);
        }

        public Task<RnetDataItem> GetAsync(RnetPath path)
        {
            return items.GetAsync(i => i.Path == path, path);
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
        public void WriteBegin(RnetPath path)
        {
            var item = GetData(path);
            if (item == null)
                items.Add(item = new RnetDataItem(path));

            item.WriteBegin();
        }

        /// <summary>
        /// Appends the data to the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        public void Write(RnetPath path, byte[] buffer)
        {
            var item = GetData(path);
            if (item == null)
                throw new NullReferenceException();

            item.Write(buffer);
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
                items.Remove(item);
        }

        /// <summary>
        /// Raised when a subscriber for a path is added.
        /// </summary>
        internal event EventHandler<ValueEventArgs<RnetPath>> SubscriberAdded;

        /// <summary>
        /// Raises the SubscriberAdded event.
        /// </summary>
        /// <param name="path"></param>
        void RaiseSubscriberAdded(RnetPath path)
        {
            if (SubscriberAdded != null)
                SubscriberAdded(this, new ValueEventArgs<RnetPath>(path));
        }

        public IEnumerator<RnetDataItem> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

}
