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
    /// Represents a directory on the device.
    /// </summary>
    public class RnetDeviceDirectory : RnetModelObject, IEnumerable<RnetDeviceDirectory>, INotifyCollectionChanged
    {

        SortedDictionary<byte, RnetDeviceDirectory> directories =
            new SortedDictionary<byte, RnetDeviceDirectory>();

        RnetDeviceData data;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="parent"></param>
        /// <param name="path"></param>
        internal RnetDeviceDirectory(RnetDevice device, RnetDeviceDirectory parent, RnetPath path)
        {
            if (device == null)
                throw new ArgumentNullException("device");

            Device = device;
            Parent = parent;
            Path = path;
        }

        /// <summary>
        /// Device that owns this directory.
        /// </summary>
        public RnetDevice Device { get; private set; }

        /// <summary>
        /// Full path of this directory.
        /// </summary>
        public RnetPath Path { get; private set; }

        /// <summary>
        /// Parent of this item.
        /// </summary>
        public RnetDeviceDirectory Parent { get; private set; }

        /// <summary>
        /// Data at the path.
        /// </summary>
        public RnetDeviceData Data
        {
            get { return data; }
            internal set { data = value; RaisePropertyChanged("Data"); }
        }

        /// <summary>
        /// Gets the directory at the specified index inside this directory, if already retrieved from the device.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public RnetDeviceDirectory this[byte index]
        {
            get
            {
                lock (directories)
                    return directories.ValueOrDefault(index);
            }
        }

        /// <summary>
        /// Gets the directory at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Task<RnetDeviceDirectory> GetAsync(byte index)
        {
            return GetAsync(index, RnetBus.CreateDefaultCancellationToken());
        }

        /// <summary>
        /// Gets the directory at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<RnetDeviceDirectory> GetAsync(byte index, CancellationToken cancellationToken)
        {
            // directory node already exists
            var directory = this[index];
            if (directory != null)
                return directory;

            // retrieve data
            var data = await Device.Data.GetAsync(Path.Navigate(index), cancellationToken);
            if (data == null)
                return GetOrCreate(index);

            // incorporate into directory
            return Add(data);
        }

        /// <summary>
        /// Gets the directory at the specified sub-path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<RnetDeviceDirectory> GetAsync(params byte[] path)
        {
            return GetAsync(RnetBus.CreateDefaultCancellationToken(), path);
        }

        /// <summary>
        /// Gets the directory at the specified sub-path.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<RnetDeviceDirectory> GetAsync(CancellationToken cancellationToken, params byte[] path)
        {
            // recurse through each directory in the path
            var directory = this;
            foreach (var p in path)
                directory = await directory.GetAsync(p, cancellationToken);

            return (RnetDeviceDirectory)directory;
        }

        /// <summary>
        /// Adds a new directory item at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        RnetDeviceDirectory Add(RnetDeviceData data)
        {
            // check prefix of data to match with ourselves
            var prefix = data.Path.Take(Path.Length);
            if (!prefix.SequenceEqual(Path))
                throw new InvalidOperationException("Adding data at inappropriate directory node.");

            // path of data relative to this
            var tail = data.Path.Skip(Path.Length).ToList();
            var directory = GetOrCreate(tail[0]);

            if (tail.Count == 1)
            {
                // directory is final resting place
                directory.Data = data;
                return directory;
            }
            else
                // dispatch to sub-directory
                return directory.Add(data);
        }

        /// <summary>
        /// Gets or creates the directory at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        RnetDeviceDirectory GetOrCreate(byte index)
        {
            lock (directories)
            {
                var directory = directories.ValueOrDefault(index);
                if (directory == null)
                {
                    directory = directories[index] = new RnetDeviceDirectory(Device, this, Path.Navigate(index));
                    RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }

                return directory;
            }
        }

        /// <summary>
        /// Removes the specified data item at the specified sub-folder.
        /// </summary>
        /// <param name="index"></param>
        void Remove(byte index)
        {
            //lock (directories)
            //{
            //    if (!directories.ContainsKey(index))
            //        return;

            //    // find existing index of device item
            //    var p = directories
            //        .Select((i,j) => new { Position = j, Index = i.Key})
            //        .Where(j => j.Index == index)
            //        .First();

            //    if (directories.Remove(index))
            //        RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, ))
            //}
        }

        /// <summary>
        /// Returns an enumerator that iterates through the items in this directory.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<RnetDeviceDirectory> GetEnumerator()
        {
            return directories.Values.GetEnumerator();
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
