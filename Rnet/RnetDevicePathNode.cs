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
    /// Represents a node in the path structure of the device.
    /// </summary>
    public class RnetDevicePathNode : RnetModelObject, IEnumerable<RnetDevicePathNode>, INotifyCollectionChanged
    {

        AsyncReaderWriterLock rw = new AsyncReaderWriterLock();
        AsyncMonitor monitor = new AsyncMonitor();

        SortedDictionary<byte, RnetDevicePathNode> nodes =
            new SortedDictionary<byte, RnetDevicePathNode>();

        byte[] buffer;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="parent"></param>
        /// <param name="path"></param>
        internal RnetDevicePathNode(RnetDevice device, RnetDevicePathNode parent, RnetPath path)
        {
            if (device == null)
                throw new ArgumentNullException("device");

            Device = device;
            Parent = parent;
            Path = path;
        }

        /// <summary>
        /// Device that owns this node.
        /// </summary>
        public RnetDevice Device { get; private set; }

        /// <summary>
        /// Full path of this node.
        /// </summary>
        public RnetPath Path { get; private set; }

        /// <summary>
        /// Parent of this item.
        /// </summary>
        public RnetDevicePathNode Parent { get; private set; }

        /// <summary>
        /// Sets the data item of the node.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal async Task SetBufferAsync(byte[] buffer)
        {
            using (await rw.ReaderLockAsync())
            {
                // do check for duplicates to avoid raising events
                if (buffer.ArrayEquals(this.buffer))
                    return;

                // replace the value
                this.buffer = buffer;

                // notify any waiters
                using (await monitor.EnterAsync())
                    monitor.PulseAll();
            }

            RaiseBufferChanged(new ValueEventArgs<byte[]>(buffer));
        }

        /// <summary>
        /// Sets the data item of the node at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        internal async Task SetBufferAsync(byte index, byte[] buffer)
        {
            var d = await FindOrCreateAsync(index, CancellationToken.None);
            if (d != null)
                await d.SetBufferAsync(buffer);
        }

        /// <summary>
        /// Sets the data item of the node at the specified relative path.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        internal async Task SetBufferAsync(byte[] buffer, params byte[] path)
        {
            var d = this;
            foreach (var p in path)
                if ((d = await d.FindOrCreateAsync(p, CancellationToken.None)) == null)
                    return;

            await d.SetBufferAsync(buffer);
        }

        /// <summary>
        /// Gets the data in the currently local node structure. Returns <c>null</c> if the data is not yet
        /// available locally.
        /// </summary>
        /// <returns></returns>
        internal Task<byte[]> FindBufferAsync()
        {
            return FindBufferAsync(Device.RequestDataCancellationToken);
        }

        /// <summary>
        /// Gets the data in the currently local node structure. Returns <c>null</c> if the data is not yet
        /// available locally.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal Task<byte[]> FindBufferAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(buffer);
        }

        /// <summary>
        /// Gets the directory at the index in the current local node structure. Returns <c>null</c> if the
        /// directory does not yet exist locally.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal Task<RnetDevicePathNode> FindAsync(byte index)
        {
            return FindAsync(index, CancellationToken.None);
        }

        /// <summary>
        /// Gets the directory at the index in the current local node structure. Returns <c>null</c> if the
        /// directory does not yet exist locally.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal async Task<RnetDevicePathNode> FindAsync(byte index, CancellationToken cancellationToken)
        {
            using (await rw.ReaderLockAsync())
                return nodes.GetOrDefault(index);
        }

        /// <summary>
        /// Gets the directory at the path in the current local node structure. Returns <c>null</c> if the
        /// directory does not yet exist locally.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal Task<RnetDevicePathNode> FindAsync(params byte[] path)
        {
            return FindAsync(CancellationToken.None, path);
        }

        /// <summary>
        /// Gets the directory at the specified relative path in the current local node structure. Returns
        /// <c>null</c> if the node does not yet exist locally.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal async Task<RnetDevicePathNode> FindAsync(CancellationToken cancellationToken, params byte[] path)
        {
            var d = this;
            foreach (var i in path)
                if ((d = await d.FindAsync(i, cancellationToken)) == null)
                    break;

            return d;
        }

        /// <summary>
        /// Gets the node at the specified index or creates it if it does not yet exist.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal async Task<RnetDevicePathNode> FindOrCreateAsync(byte index, CancellationToken cancellationToken)
        {
            using (var read = await rw.UpgradeableReaderLockAsync(cancellationToken))
            {
                var directory = await FindAsync(index);
                if (directory == null)
                {
                    using (await read.UpgradeAsync())
                        directory = nodes[index] = new RnetDevicePathNode(Device, this, Path.Navigate(index));

                    // post collection change notification
                    Device.Bus.SynchronizationContext.Post(state =>
                        RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)),
                        null);

                    // notify potentially waiting parties
                    using (await monitor.EnterAsync(cancellationToken))
                        monitor.PulseAll();
                }

                return directory;
            }
        }

        /// <summary>
        /// Gets the node at the specified relative path or creates it if it does not yet exist.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        internal async Task<RnetDevicePathNode> FindOrCreateAsync(CancellationToken cancellationToken, params byte[] path)
        {
            var d = this;
            foreach (var i in path)
                if ((d = await d.FindOrCreateAsync(i, cancellationToken)) == null)
                    break;

            return d;
        }

        /// <summary>
        /// Reads the data in this node from the device.
        /// </summary>
        /// <returns></returns>
        public Task<byte[]> RequestBufferAsync()
        {
            return RequestBufferAsync(Device.RequestDataCancellationToken);
        }

        /// <summary>
        /// Reads the data in this node from the device.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<byte[]> RequestBufferAsync(CancellationToken cancellationToken)
        {
            return await (await Device.RequestAsync(Path, cancellationToken)).FindBufferAsync(cancellationToken);
        }

        /// <summary>
        /// Reads the node at the specified index from the device.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Task<RnetDevicePathNode> RequestAsync(byte index)
        {
            return RequestAsync(index, Device.RequestDataCancellationToken);
        }

        /// <summary>
        /// Reads the node at the specified index from the device.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<RnetDevicePathNode> RequestAsync(byte index, CancellationToken cancellationToken)
        {
            return Device.RequestAsync(Path.Navigate(index), cancellationToken);
        }

        /// <summary>
        /// Reads the node at the specified relative path from the device.
        /// </summary>
        /// <param name="path"></param>
        public Task<RnetDevicePathNode> RequestAsync(params byte[] path)
        {
            return RequestAsync(Device.RequestDataCancellationToken, path);
        }

        /// <summary>
        /// Reads the node at the specified relative path from the device.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<RnetDevicePathNode> RequestAsync(CancellationToken cancellationToken, params byte[] path)
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
        internal async Task<byte[]> WaitBufferAsync(CancellationToken cancellationToken)
        {
            byte[] buffer = null;
            while ((buffer = await FindBufferAsync(cancellationToken)) == null && !cancellationToken.IsCancellationRequested)
                using (await monitor.EnterAsync(cancellationToken))
                    await monitor.WaitAsync(cancellationToken);

            return buffer;
        }

        /// <summary>
        /// Waits for the node at the specified index to appear.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal async Task<RnetDevicePathNode> WaitAsync(byte index, CancellationToken cancellationToken)
        {
            var directory = this;
            while ((directory = await FindAsync(index, cancellationToken)) == null && !cancellationToken.IsCancellationRequested)
                using (await monitor.EnterAsync(cancellationToken))
                    await monitor.WaitAsync(cancellationToken);

            return directory;
        }

        /// <summary>
        /// Waits for the node at the specified relative path to appear.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        internal async Task<RnetDevicePathNode> WaitAsync(CancellationToken cancellationToken, params byte[] path)
        {
            var d = this;
            foreach (var p in path)
                if ((d = await d.WaitAsync(p, cancellationToken)) == null || cancellationToken.IsCancellationRequested)
                    return null;

            return d;
        }

        /// <summary>
        /// Gets the node data.
        /// </summary>
        /// <returns></returns>
        public async Task<byte[]> GetBufferAsync()
        {
            try
            {
                return await GetBufferAsync(Device.RequestDataCancellationToken);
            }
            catch (OperationCanceledException)
            {
                // ignore
            }

            return null;
        }

        /// <summary>
        /// Gets the node data.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<byte[]> GetBufferAsync(CancellationToken cancellationToken)
        {
            return WaitBufferAsync(cancellationToken);
        }

        /// <summary>
        /// Gets the node data at the specified relative index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task<byte[]> GetBufferAsync(byte index)
        {
            try
            {
                return await GetBufferAsync(index, Device.RequestDataCancellationToken);
            }
            catch (OperationCanceledException)
            {
                // ignore
            }

            return null;
        }

        /// <summary>
        /// Gets the node data at the specified relative index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<byte[]> GetBufferAsync(byte index, CancellationToken cancellationToken)
        {
            var d = await GetAsync(index, cancellationToken);
            if (d != null)
                return await d.GetBufferAsync();

            return null;
        }

        /// <summary>
        /// Gets the node data at the specified relative path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<byte[]> GetBufferAsync(params byte[] path)
        {
            try
            {
                return await GetBufferAsync(Device.RequestDataCancellationToken, path);
            }
            catch (OperationCanceledException)
            {
                // ignore
            }

            return null;
        }

        /// <summary>
        /// Gets the node data at the specified relative path.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<byte[]> GetBufferAsync(CancellationToken cancellationToken, params byte[] path)
        {
            var d = await GetAsync(cancellationToken, path);
            if (d != null)
                return await d.GetBufferAsync();

            return null;
        }

        /// <summary>
        /// Gets the node at the specified index if available or requests it from the remote device.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task<RnetDevicePathNode> GetAsync(byte index)
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
        /// Gets the node at the specified index if available or requests it from the remote device.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<RnetDevicePathNode> GetAsync(byte index, CancellationToken cancellationToken)
        {
            var directory = await FindAsync(index, cancellationToken);
            if (directory == null)
                directory = await RequestAsync(index, cancellationToken);

            return directory;
        }

        /// <summary>
        /// Gets the node at the specified relative path if available or requests it from the remote device.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<RnetDevicePathNode> GetAsync(params byte[] path)
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
        /// Gets the node at the specified relative path if available or requests it from the remote device.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<RnetDevicePathNode> GetAsync(CancellationToken cancellationToken, params byte[] path)
        {
            var directory = await FindAsync(cancellationToken, path);
            if (directory == null)
                directory = await RequestAsync(cancellationToken, path);

            return directory;
        }

        /// <summary>
        /// Writes the data to the node.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public Task WriteAsync(byte[] buffer)
        {
            return WriteAsync(buffer, Device.SetDataCancellationToken);
        }

        /// <summary>
        /// Writes the data to the node.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task WriteAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            await Device.WriteAsync(Path, buffer, cancellationToken);
            await Device.RequestAsync(Path, cancellationToken);
        }

        /// <summary>
        /// Writes the single byte to the node.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Task WriteAsync(byte value)
        {
            return WriteAsync(value, Device.SetDataCancellationToken);
        }

        /// <summary>
        /// Writes the single byte to the node.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task WriteAsync(byte value, CancellationToken cancellationToken)
        {
            return WriteAsync(new[] { value }, cancellationToken);
        }

        /// <summary>
        /// Raises an event against this node in the device.
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        public Task RaiseEvent(RnetEvent evt)
        {
            return RaiseEvent(evt, 0, 0, RnetPriority.Low, Device.RequestDataCancellationToken);
        }

        /// <summary>
        /// Raises an event against this node in the device.
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        public Task RaiseEvent(RnetEvent evt, CancellationToken cancellationToken)
        {
            return RaiseEvent(evt, 0, 0, RnetPriority.Low, cancellationToken);
        }

        /// <summary>
        /// Raises an event against this node in the device.
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        public Task RaiseEvent(RnetEvent evt, ushort timestamp, ushort data)
        {
            return RaiseEvent(evt, timestamp, data, RnetPriority.Low, Device.RequestDataCancellationToken);
        }

        /// <summary>
        /// Raises an event against this node in the device.
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        public async Task RaiseEvent(RnetEvent evt, ushort timestamp, ushort data, RnetPriority priority, CancellationToken cancellationToken)
        {
            await Device.RaiseEvent(Path, evt, timestamp, data, priority);
            await Device.RequestAsync(Path, cancellationToken);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the items in this node.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<RnetDevicePathNode> GetEnumerator()
        {
            return nodes.Values.ToList().GetEnumerator();
        }

        /// <summary>
        /// Raised when the data buffer stored in the node is changed.
        /// </summary>
        public event EventHandler<ValueEventArgs<byte[]>> BufferChanged;

        /// <summary>
        /// Raises the BufferChanged event.
        /// </summary>
        /// <param name="args"></param>
        void RaiseBufferChanged(ValueEventArgs<byte[]> args)
        {
            if (BufferChanged != null)
                BufferChanged(this, args);
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

}
