using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Rnet
{

    /// <summary>
    /// Represents the root of a data item tree.
    /// </summary>
    public class RnetDataTreeRoot : RnetDataTreeNode
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="item"></param>
        internal RnetDataTreeRoot(RnetDevice device)
            : base(device)
        {
            Device.DataItems.CollectionChanged += DataItems_CollectionChanged;
        }

        /// <summary>
        /// Invoked when the data items collection is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void DataItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Add(args.NewItems.Cast<RnetDataItem>());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Remove(args.OldItems.Cast<RnetDataItem>());
                    break;
                case NotifyCollectionChangedAction.Replace:
                    Remove(args.OldItems.Cast<RnetDataItem>());
                    Add(args.NewItems.Cast<RnetDataItem>());
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Remove(Device.DataItems);
                    Add(Device.DataItems);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Constructs the path for the given child folder.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        internal override RnetPath GetChildPath(byte folder)
        {
            return new RnetPath(folder);
        }

        /// <summary>
        /// Adds the set of items to the tree.
        /// </summary>
        /// <param name="items"></param>
        void Add(IEnumerable<RnetDataItem> items)
        {
            foreach (var item in items)
                Add(item);
        }

        /// <summary>
        /// Adds the item to the tree.
        /// </summary>
        /// <param name="item"></param>
        void Add(RnetDataItem item)
        {
            var node = (RnetDataTreeNode)this;
            foreach (var p in item.Path.Take(item.Path.Length - 1))
                node = node[p] ?? node.SetItem(p, null);

            node.SetItem(item.Path[item.Path.Length - 1], item);
        }

        /// <summary>
        /// Removes the set of items from the tree.
        /// </summary>
        /// <param name="items"></param>
        void Remove(IEnumerable<RnetDataItem> items)
        {
            foreach (var item in items)
                Remove(item);
        }

        /// <summary>
        /// Removes the item from the tree.
        /// </summary>
        /// <param name="item"></param>
        void Remove(RnetDataItem item)
        {
            var node = (RnetDataTreeNode)this;
            foreach (var p in item.Path.Take(item.Path.Length - 1))
                if ((node = node[p]) == null)
                    return;

            node.RemoveItem(item.Path[item.Path.Length - 1]);
        }

    }

}
