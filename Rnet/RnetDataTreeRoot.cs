using System.Collections.Generic;
using System.Linq;

namespace Rnet
{

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

        }

        internal override RnetPath GetChildPath(byte folder)
        {
            return new RnetPath(folder);
        }

        internal void Remove(IEnumerable<RnetDataItem> items)
        {
            foreach (var item in items)
                Remove(item);
        }

        internal void Remove(RnetDataItem item)
        {
            var node = (RnetDataTreeNode)this;
            foreach (var p in item.Path.Take(item.Path.Length - 1))
                if ((node = node[p]) == null)
                    return;

            node.RemoveItem(item.Path[item.Path.Length - 1]);
        }

        internal void Add(IEnumerable<RnetDataItem> items)
        {
            foreach (var item in items)
                Add(item);
        }

        internal void Add(RnetDataItem item)
        {
            var node = (RnetDataTreeNode)this;
            foreach (var p in item.Path.Take(item.Path.Length - 1))
                node = node[p] ?? node.SetItem(p, null);

            node.SetItem(item.Path[item.Path.Length - 1], item);
        }

    }

}
