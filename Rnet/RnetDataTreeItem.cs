namespace Rnet
{

    public class RnetDataTreeItem : RnetDataTreeNode
    {

        RnetDataTreeNode parent;
        RnetPath path;
        RnetDataItem item;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="item"></param>
        internal RnetDataTreeItem(RnetDevice device, RnetDataTreeNode parent, RnetPath path, RnetDataItem item)
            : base(device)
        {
            Parent = parent;
            Path = path;
            Item = item;
        }

        /// <summary>
        /// Parent of this item.
        /// </summary>
        public RnetDataTreeNode Parent
        {
            get { return parent; }
            private set { parent = value; RaisePropertyChanged("Parent"); }
        }

        /// <summary>
        /// Path at which the tree item resides.
        /// </summary>
        public RnetPath Path
        {
            get { return path; }
            private set { path = value; RaisePropertyChanged("Path"); }
        }

        /// <summary>
        /// Item at the path.
        /// </summary>
        public RnetDataItem Item
        {
            get { return item; }
            private set { item = value; RaisePropertyChanged("Item"); }
        }

        internal override RnetPath GetChildPath(byte folder)
        {
            return Path.Child(folder);
        }

    }

}
