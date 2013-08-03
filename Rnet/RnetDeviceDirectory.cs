using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Nito.AsyncEx;

namespace Rnet
{

    /// <summary>
    /// Represents a directory on the device.
    /// </summary>
    public class RnetDeviceDirectory : RnetModelObject, IEnumerable<RnetDeviceDirectory>, INotifyCollectionChanged
    {

        AsyncLock asyncLock = new AsyncLock();
        AsyncMonitor monitor = new AsyncMonitor();
        SortedDictionary<byte, RnetDeviceDirectory> directories =
            new SortedDictionary<byte, RnetDeviceDirectory>();
        byte[] buffer;

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
        public byte[] Buffer
        {
            get { return buffer; }
            private set { buffer = value; RaisePropertyChanged("Buffer"); }
        }

        /// <summary>
        /// Gets the data at the index in the current local directory structure. Returns <c>null</c> if the data does
        /// not yet exist locally.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public RnetDeviceDirectory this[byte index]
        {
            get { return FindAsync(index).Result; }
        }

        /// <summary>
        /// Sets the data item of the directory.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal async Task SetAsync(byte[] buffer)
        {
            Buffer = buffer;

            // notify any waiters
            using (await monitor.EnterAsync())
                monitor.PulseAll();
        }

        /// <summary>
        /// Sets the data item of the directory at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="buffer"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal async Task SetAsync(byte index, byte[] buffer)
        {
            var d = await FindOrCreateAsync(index, CancellationToken.None);
            if (d != null)
                await d.SetAsync(buffer);
        }

        /// <summary>
        /// Sets the data item of the directory at the specified relative path.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        internal async Task SetAsync(byte[] buffer, params byte[] path)
        {
            var d = this;
            foreach (var p in path)
                if ((d = await d.FindOrCreateAsync(p, CancellationToken.None)) == null)
                    return;

            await d.SetAsync(buffer);
        }

        /// <summary>
        /// Gets the data in the currently local directory structure. Returns <c>null</c> if the data is not yet
        /// available locally.
        /// </summary>
        /// <returns></returns>
        public Task<byte[]> FindAsync()
        {
            return FindAsync(Device.RequestDataCancellationToken);
        }

        /// <summary>
        /// Gets the data in the currently local directory structure. Returns <c>null</c> if the data is not yet
        /// available locally.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<byte[]> FindAsync(CancellationToken cancellationToken)
        {
            using (await monitor.EnterAsync(cancellationToken))
                return buffer;
        }

        /// <summary>
        /// Gets the directory at the index in the current local directory structure. Returns <c>null</c> if the
        /// directory does not yet exist locally.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task<RnetDeviceDirectory> FindAsync(byte index, CancellationToken cancellationToken)
        {
            using (await monitor.EnterAsync(cancellationToken))
                return directories.GetOrDefault(index);
        }

        /// <summary>
        /// Gets the directory at the path in the current local directory structure. Returns <c>null</c> if the
        /// directory does not yet exist locally.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<RnetDeviceDirectory> FindAsync(params byte[] path)
        {
            return FindAsync(Device.RequestDataCancellationToken, path);
        }

        /// <summary>
        /// Gets the directory at the specified relative path in the current local directory structure. Returns
        /// <c>null</c> if the directory does not yet exist locally.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<RnetDeviceDirectory> FindAsync(CancellationToken cancellationToken, params byte[] path)
        {
            var d = this;
            foreach (var i in path)
                if ((d = await d.FindAsync(i, cancellationToken)) == null)
                    break;

            return d;
        }

        /// <summary>
        /// Gets the directory at the specified index or creates it if it does not yet exist.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal async Task<RnetDeviceDirectory> FindOrCreateAsync(byte index, CancellationToken cancellationToken)
        {
            using (await monitor.EnterAsync(cancellationToken))
            {
                var directory = directories.GetOrDefault(index);
                if (directory == null)
                {
                    directory = directories[index] = new RnetDeviceDirectory(Device, this, Path.Navigate(index));
                    RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    monitor.PulseAll();
                }

                return directory;
            }
        }

        /// <summary>
        /// Gets the directory at the specified relative path or creates it if it does not yet exist.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        internal async Task<RnetDeviceDirectory> FindOrCreateAsync(CancellationToken cancellationToken, params byte[] path)
        {
            var d = this;
            foreach (var i in path)
                if ((d = await d.FindOrCreateAsync(i, cancellationToken)) == null)
                    break;

            return d;
        }

        /// <summary>
        /// Reads the data in this directory from the device.
        /// </summary>
        /// <returns></returns>
        public Task<byte[]> RequestAsync()
        {
            return RequestAsync(Device.RequestDataCancellationToken);
        }

        /// <summary>
        /// Reads the data in this directory from the device.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<byte[]> RequestAsync(CancellationToken cancellationToken)
        {
            return await (await Device.RequestAsync(Path, cancellationToken)).FindAsync(cancellationToken);
        }

        /// <summary>
        /// Reads the directory at the specified index from the device.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Task<RnetDeviceDirectory> RequestAsync(byte index)
        {
            return RequestAsync(index, Device.RequestDataCancellationToken);
        }

        /// <summary>
        /// Reads the directory at the specified index from the device.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<RnetDeviceDirectory> RequestAsync(byte index, CancellationToken cancellationToken)
        {
            return Device.RequestAsync(Path.Navigate(index), cancellationToken);
        }

        /// <summary>
        /// Reads the directory at the specified relative path from the device.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<RnetDeviceDirectory> RequestAsync(CancellationToken cancellationToken, params byte[] path)
        {
            var p = Path;
            foreach (var i in path)
                p = p.Navigate(i);

            return Device.RequestAsync(p, cancellationToken);
        }

        /// <summary>
        /// Waits for the data to be available.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal async Task<byte[]> WaitAsync(CancellationToken cancellationToken)
        {
            byte[] buffer = null;
            while ((buffer = await FindAsync(cancellationToken)) == null && !cancellationToken.IsCancellationRequested)
                using (await monitor.EnterAsync(cancellationToken))
                    await monitor.WaitAsync(cancellationToken);

            return buffer;
        }

        /// <summary>
        /// Waits for the directory at the specified index to appear.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal async Task<RnetDeviceDirectory> WaitAsync(byte index, CancellationToken cancellationToken)
        {
            var directory = this;
            while ((directory = await FindAsync(index, cancellationToken)) == null && !cancellationToken.IsCancellationRequested)
                using (await monitor.EnterAsync(cancellationToken))
                    await monitor.WaitAsync(cancellationToken);

            return directory;
        }

        /// <summary>
        /// Waits for the directory at the specified relative path to appear.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        internal async Task<RnetDeviceDirectory> WaitAsync(CancellationToken cancellationToken, params byte[] path)
        {
            var d = this;
            foreach (var p in path)
                if ((d = await d.WaitAsync(p, cancellationToken)) == null || cancellationToken.IsCancellationRequested)
                    return null;

            return d;
        }

        /// <summary>
        /// Writes the data to the directory.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public Task WriteAsync(byte[] buffer)
        {
            return WriteAsync(buffer, Device.RequestDataCancellationToken);
        }

        /// <summary>
        /// Writes the data to the directory.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task WriteAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            return Device.WriteAsync(Path, buffer, cancellationToken);
        }

        /// <summary>
        /// Gets the directory data.
        /// </summary>
        /// <returns></returns>
        public async Task<byte[]> GetAsync()
        {
            try
            {
                return await GetAsync(Device.RequestDataCancellationToken);
            }
            catch (OperationCanceledException)
            {
                // ignore
            }

            return null;
        }

        /// <summary>
        /// Gets the directory data.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<byte[]> GetAsync(CancellationToken cancellationToken)
        {
            return WaitAsync(cancellationToken);
        }

        /// <summary>
        /// Gets the directory at the specified index if available or requests it from the remote device.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task<RnetDeviceDirectory> GetAsync(byte index)
        {
            try
            {
                return await GetAsync(index, Device.RequestDataCancellationToken);
            }
            catch (OperationCanceledException)
            {
                // ignore
            }

            return null;
        }

        /// <summary>
        /// Gets the directory at the specified index if available or requests it from the remote device.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<RnetDeviceDirectory> GetAsync(byte index, CancellationToken cancellationToken)
        {
            var directory = await FindAsync(index, cancellationToken);
            if (directory == null)
                directory = await RequestAsync(index, cancellationToken);

            return directory;
        }

        /// <summary>
        /// Gets the directory at the specified relative path if available or requests it from the remote device.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<RnetDeviceDirectory> GetAsync(params byte[] path)
        {
            try
            {
                return await GetAsync(Device.RequestDataCancellationToken, path);
            }
            catch (OperationCanceledException)
            {
                // ignore
            }

            return null;
        }

        /// <summary>
        /// Gets the directory at the specified relative path if available or requests it from the remote device.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<RnetDeviceDirectory> GetAsync(CancellationToken cancellationToken, params byte[] path)
        {
            var directory = await FindAsync(cancellationToken, path);
            if (directory == null)
                directory = await RequestAsync(cancellationToken, path);

            return directory;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the items in this directory.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<RnetDeviceDirectory> GetEnumerator()
        {
            return directories.Values.ToList().GetEnumerator();
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
