using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

using System.Linq;
using System.Reactive.Linq;
using System.Reactive;
using System.Diagnostics;

namespace Rnet
{

    public static class RnetDirectoryWatcherExtensions
    {

        /// <summary>
        /// Establish a watch at the specified directory position.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static RnetDirectoryWatcher Watch(this RnetDeviceDirectory directory, Action<RnetDirectoryWatcherParent> watch)
        {
            var w = new RnetDirectoryWatcher(directory, watch);
            w.Bind();
            return w;
        }

    }

    /// <summary>
    /// Base of watcher type hierarchy. A node contains a parent directory and current directory.
    /// </summary>
    public abstract class RnetDirectoryWatcherNode
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected RnetDirectoryWatcherNode()
        {

        }

        /// <summary>
        /// Gets the current directory which this node is bound to.
        /// </summary>
        protected RnetDeviceDirectory ParentDirectory { get; private set; }

        /// <summary>
        /// Gets the current directory which this node is bound to.
        /// </summary>
        protected RnetDeviceDirectory Directory { get; set; }

        /// <summary>
        /// Accepts the parent directory for this node.
        /// </summary>
        /// <param name="parentDirectory"></param>
        protected internal virtual void SetParentDirectory(RnetDeviceDirectory parentDirectory)
        {
            if (ParentDirectory != null)
            {
                ParentDirectory.CollectionChanged -= ParentDirectory_CollectionChanged;
                ParentDirectory.BufferChanged -= ParentDirectory_BufferChanged;
            }

            ParentDirectory = parentDirectory;

            if (ParentDirectory != null)
            {
                ParentDirectory.BufferChanged += ParentDirectory_BufferChanged;
                ParentDirectory.CollectionChanged += ParentDirectory_CollectionChanged;
            }

            OnParentDirectoryChanged();
        }

        /// <summary>
        /// Invoke when a new current directory is known.
        /// </summary>
        protected virtual void SetDirectory(RnetDeviceDirectory directory)
        {
            Directory = directory;
            OnDirectoryChanged();
        }

        /// <summary>
        /// Invoked when the parent directories buffer is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void ParentDirectory_BufferChanged(object sender, ValueEventArgs<byte[]> args)
        {
            OnParentDirectoryBufferChanged();
        }

        /// <summary>
        /// Invoked when the collection changes on the parent directory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void ParentDirectory_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            OnParentDirectoryCollectionChanged();
        }

        /// <summary>
        /// Invoked when the current directory is changed.
        /// </summary>
        protected virtual void OnDirectoryChanged()
        {

        }

        /// <summary>
        /// Invoked when the parent directory is changed.
        /// </summary>
        protected virtual void OnParentDirectoryChanged()
        {

        }

        /// <summary>
        /// Invoked when the parent directories buffer is changed.
        /// </summary>
        protected virtual void OnParentDirectoryBufferChanged()
        {

        }

        /// <summary>
        /// Invoked when the collection changes on the parent directory.
        /// </summary>
        protected virtual void OnParentDirectoryCollectionChanged()
        {

        }

        /// <summary>
        /// Returns all of the watched paths. Invoked with the path of the parent directory.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected internal virtual IEnumerable<RnetPath> GetWatchedPathsWithParent(RnetPath path)
        {
            yield break;
        }

        /// <summary>
        /// Returns all of the watched paths. Invoked with the path of the current directory.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected internal virtual IEnumerable<RnetPath> GetWatchedPaths(RnetPath path)
        {
            yield break;
        }

    }

    /// <summary>
    /// Watcher class that contains other node items.
    /// </summary>
    public abstract class RnetDirectoryWatcherParent : RnetDirectoryWatcherNode
    {

        List<RnetDirectoryWatcherNode> nodes =
            new List<RnetDirectoryWatcherNode>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected RnetDirectoryWatcherParent()
            : base()
        {

        }

        public RnetDirectoryWatcherParent When(byte a, Action<RnetDirectoryWatcherWhen> when)
        {
            var node = new RnetDirectoryWatcherWhen(a);
            nodes.Add(node);
            when(node);
            return this;
        }

        public RnetDirectoryWatcherParent When(byte a, byte b, Action<RnetDirectoryWatcherWhen> when)
        {
            return
                When(a, w => w
                    .When(b, x => when(x)));
        }

        public RnetDirectoryWatcherParent When(byte a, byte b, byte c, Action<RnetDirectoryWatcherWhen> when)
        {
            return
                When(a, w => w
                    .When(b, x => x
                        .When(c, y => when(y))));
        }

        public RnetDirectoryWatcherParent When(byte a, byte b, byte c, byte d, Action<RnetDirectoryWatcherWhen> when)
        {
            return
                When(a, w => w
                    .When(b, x => x
                        .When(c, y => y
                            .When(d, z => when(z)))));
        }

        /// <summary>
        /// Performs the action when the path is changed.
        /// </summary>
        /// <param name="then"></param>
        /// <returns></returns>
        public RnetDirectoryWatcherParent Then(Action<byte[]> then)
        {
            nodes.Add(new RnetDirectoryWatcherThen(then));
            return this;
        }

        protected override void OnDirectoryChanged()
        {
            base.OnDirectoryChanged();

            // update child nodes
            foreach (var node in nodes)
                node.SetParentDirectory(Directory);
        }

        protected internal override IEnumerable<RnetPath> GetWatchedPaths(RnetPath path)
        {
            // send to child nodes first
            foreach (var node in nodes)
                foreach (var path2 in node.GetWatchedPathsWithParent(path))
                    yield return path2;

            foreach (var path2 in base.GetWatchedPaths(path))
                yield return path2;
        }

    }

    /// <summary>
    /// Serves as the root of the watcher hierarchy. Does not contain a parent directory.
    /// </summary>
    public class RnetDirectoryWatcher : RnetDirectoryWatcherParent
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="watch"></param>
        internal RnetDirectoryWatcher(RnetDeviceDirectory directory, Action<RnetDirectoryWatcherParent> watch)
            : base()
        {
            Directory = directory;
            watch(this);
        }

        /// <summary>
        /// Binds to the current directory structure.
        /// </summary>
        internal void Bind()
        {
            SetDirectory(Directory);
        }

        /// <summary>
        /// Initiates requests to load all of the watched values.
        /// </summary>
        /// <returns></returns>
        public async Task LoadAsync()
        {
            foreach (var path in GetWatchedPaths())
                await Directory.Device.Directory.RequestAsync(path);
        }

        /// <summary>
        /// Returns an enumeration of all of the watched paths.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RnetPath> GetWatchedPaths()
        {
            return base.GetWatchedPaths(Directory.Path);
        }

    }

    /// <summary>
    /// Serves as a child item and a collection. The index determines the current item, which becomes the parent item of children.
    /// </summary>
    public class RnetDirectoryWatcherWhen : RnetDirectoryWatcherParent
    {

        byte index;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="index"></param>
        internal RnetDirectoryWatcherWhen(byte index)
            : base()
        {
            this.index = index;
        }

        /// <summary>
        /// Invoked when the parent directory is changed.
        /// </summary>
        protected override void OnParentDirectoryChanged()
        {
            SetDirectory();
        }

        /// <summary>
        /// Invoked when an item of the parent directory is changed.
        /// </summary>
        protected override void OnParentDirectoryCollectionChanged()
        {
            SetDirectory();
        }

        /// <summary>
        /// Determines the new current directory based on the index.
        /// </summary>
        void SetDirectory()
        {
            SetDirectory(ParentDirectory != null ? ParentDirectory[index] : null);
        }

        protected internal override IEnumerable<RnetPath> GetWatchedPathsWithParent(RnetPath path)
        {
            return
                Enumerable.Empty<RnetPath>()
                    .Concat(base.GetWatchedPathsWithParent(path))
                    .Concat(base.GetWatchedPaths(path.Navigate(index)));
        }

    }

    /// <summary>
    /// Serves as a child item that fires off teh specified action in response to a change in buffer value on the parent directory.
    /// </summary>
    public class RnetDirectoryWatcherThen : RnetDirectoryWatcherNode
    {

        Action<byte[]> action;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="action"></param>
        internal RnetDirectoryWatcherThen(Action<byte[]> action)
            : base()
        {
            this.action = action;
        }

        protected override void OnParentDirectoryChanged()
        {
            if (ParentDirectory != null)
                Task.Run(async () => action(await ParentDirectory.GetDataAsync()));
        }

        protected internal override IEnumerable<RnetPath> GetWatchedPathsWithParent(RnetPath path)
        {
            yield return path;
        }

    }

}
