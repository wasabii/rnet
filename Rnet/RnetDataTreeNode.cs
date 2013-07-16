using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace Rnet
{

    public abstract class RnetDataTreeNode : RnetModelObject, IEnumerable<RnetDataTreeNode>, INotifyCollectionChanged
    {

        RnetDevice device;

        /// <summary>
        /// Implementation of items.
        /// </summary>
        ObservableCollection<RnetDataTreeItem> items =
            new ObservableCollection<RnetDataTreeItem>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected RnetDataTreeNode(RnetDevice device)
        {
            Device = device;

            items.CollectionChanged += (s, a) => RaiseCollectionChanged(a);
        }

        /// <summary>
        /// Gets the child path for the given sub-folder.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        internal abstract RnetPath GetChildPath(byte folder);

        /// <summary>
        /// Gets or sets the tree node at the specified sub-folder.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public RnetDataTreeItem this[byte folder]
        {
            get { return GetItem(folder); }
        }

        /// <summary>
        /// Gets the item at the specified sub-folder.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        RnetDataTreeItem GetItem(byte folder)
        {
            lock (device)
            {
                var c = GetChildPath(folder);
                return items.FirstOrDefault(i => i.Path == c);
            }
        }

        /// <summary>
        /// Gets the item at the specified sub-folder, waiting until it is available.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public async Task<RnetDataTreeItem> GetItemAsync(byte folder)
        {
            var item = await device.DataItems.GetAsync(GetChildPath(folder));

            lock (device)
            {
                var node = GetItem(folder);
                if (node == null)
                    SetItem(folder, item);

                return node;
            }
        }

        /// <summary>
        /// Sets the specified data item at the specified sub-folder.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="item"></param>
        internal RnetDataTreeItem SetItem(byte folder, RnetDataItem item)
        {
            lock (device)
            {
                var node = GetItem(folder);
                if (node != null &&
                    node.Item != item)
                {
                    // remove mismatched item node
                    items.Remove(node);
                    node = null;
                }

                if (node == null)
                    items.Add(node = new RnetDataTreeItem(device, this, GetChildPath(folder), item));

                return node;
            }
        }

        /// <summary>
        /// Removes the specified data item at the specified sub-folder.
        /// </summary>
        /// <param name="folder"></param>
        internal void RemoveItem(byte folder)
        {
            lock (device)
            {
                var node = GetItem(folder);
                if (node == null)
                    return;

                items.Remove(node);
            }
        }

        /// <summary>
        /// Device at which the tree item resides.
        /// </summary>
        public RnetDevice Device
        {
            get { return device; }
            private set { device = value; RaisePropertyChanged("Device"); }
        }

        public IEnumerator<RnetDataTreeNode> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Raised when the items in the collection are changed.
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
