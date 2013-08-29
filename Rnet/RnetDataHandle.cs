using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rnet
{

    /// <summary>
    /// Provides a handle by which to manipulate data at a specific path on an RNET device.
    /// </summary>
    public abstract class RnetDataHandle : IObservable<byte[]>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="path"></param>
        internal protected RnetDataHandle(RnetDevice device, RnetPath path)
        {
            if (device == null)
                throw new ArgumentNullException("device");

            Device = device;
            Path = path;
        }

        /// <summary>
        /// Owning device of the handle.
        /// </summary>
        public RnetDevice Device { get; private set; }

        /// <summary>
        /// Full path of this handle.
        /// </summary>
        public RnetPath Path { get; private set; }

        /// <summary>
        /// Gets the currently cached value, without reading it from the remote device.
        /// </summary>
        public abstract byte[] Current { get; }

        /// <summary>
        /// Reads the data from the device path.
        /// </summary>
        /// <returns></returns>
        public Task<byte[]> Read()
        {
            return Read(CancellationToken.None);
        }

        /// <summary>
        /// Reads the data from the device path.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task<byte[]> Read(CancellationToken cancellationToken);

        /// <summary>
        /// Refreshes the data from the device.
        /// </summary>
        /// <returns></returns>
        public Task<byte[]> Refresh()
        {
            return Refresh(CancellationToken.None);
        }

        /// <summary>
        /// Refreshes the data from the device.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task<byte[]> Refresh(CancellationToken cancellationToken);

        /// <summary>
        /// Writes the byte to the device and returns the data after the write was completed.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Task<byte[]> Write(byte value)
        {
            return Write(value, CancellationToken.None);
        }

        /// <summary>
        /// Writes the byte to the device and returns the data after the write was completed.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<byte[]> Write(byte value, CancellationToken cancellationToken)
        {
            return Write(new[] { value }, cancellationToken);
        }

        /// <summary>
        /// Writes the data to the device and returns the data after the write was completed.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task<byte[]> Write(byte[] data)
        {
            return Write(data, CancellationToken.None);
        }

        /// <summary>
        /// Writes the data to the device and returns the data after the write was completed.
        /// </summary>
        /// <typeparam name="?"></typeparam>
        /// <param name="?"></param>
        /// <returns></returns>
        public abstract Task<byte[]> Write(byte[] data, CancellationToken cancellationToken);

        /// <summary>
        /// Sends an event to the path on the device and returns the value after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        public Task<byte[]> SendEvent(RnetEvent evt)
        {
            return SendEvent(evt, CancellationToken.None);
        }

        /// <summary>
        /// Sends an event to the path on the device and returns the value after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<byte[]> SendEvent(RnetEvent evt, CancellationToken cancellationToken)
        {
            return SendEvent(evt, (ushort)0, cancellationToken);
        }

        /// <summary>
        /// Sends an event to the path on the device and returns the value after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public Task<byte[]> SendEvent(RnetEvent evt, int timestamp)
        {
            return SendEvent(evt, timestamp, CancellationToken.None);
        }

        /// <summary>
        /// Sends an event to the path on the device and returns the value after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="timestamp"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<byte[]> SendEvent(RnetEvent evt, int timestamp, CancellationToken cancellationToken)
        {
            return checked(SendEvent(evt, (ushort)timestamp, cancellationToken));
        }

        /// <summary>
        /// Sends an event to the path on the device and returns the value after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public Task<byte[]> SendEvent(RnetEvent evt, byte timestamp)
        {
            return SendEvent(evt, timestamp, CancellationToken.None);
        }

        /// <summary>
        /// Sends an event to the path on the device and returns the value after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="timestamp"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<byte[]> SendEvent(RnetEvent evt, byte timestamp, CancellationToken cancellationToken)
        {
            return SendEvent(evt, (ushort)timestamp, cancellationToken);
        }

        /// <summary>
        /// Sends an event to the path on the device and returns the value after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public Task<byte[]> SendEvent(RnetEvent evt, ushort timestamp)
        {
            return SendEvent(evt, timestamp, CancellationToken.None);
        }

        /// <summary>
        /// Sends an event to the path on the device and returns the value after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="timestamp"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<byte[]> SendEvent(RnetEvent evt, ushort timestamp, CancellationToken cancellationToken)
        {
            return SendEvent(evt, timestamp, (ushort)0, cancellationToken);
        }

        /// <summary>
        /// Sends an event to the path on the device and returns the value after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="timestamp"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task<byte[]> SendEvent(RnetEvent evt, int timestamp, int data)
        {
            return SendEvent(evt, timestamp, data, CancellationToken.None);
        }

        /// <summary>
        /// Sends an event to the path on the device and returns the value after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="timestamp"></param>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<byte[]> SendEvent(RnetEvent evt, int timestamp, int data, CancellationToken cancellationToken)
        {
            return checked(SendEvent(evt, (ushort)timestamp, (ushort)data, cancellationToken));
        }

        /// <summary>
        /// Sends an event to the path on the device and returns the value after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="timestamp"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task<byte[]> SendEvent(RnetEvent evt, byte timestamp, byte data)
        {
            return SendEvent(evt, timestamp, data, CancellationToken.None);
        }

        /// <summary>
        /// Sends an event to the path on the device and returns the value after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="timestamp"></param>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<byte[]> SendEvent(RnetEvent evt, byte timestamp, byte data, CancellationToken cancellationToken)
        {
            return SendEvent(evt, (ushort)timestamp, (ushort)data, cancellationToken);
        }

        /// <summary>
        /// Sends an event to the path on the device and returns the value after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="timestamp"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task<byte[]> SendEvent(RnetEvent evt, ushort timestamp, ushort data)
        {
            return SendEvent(evt, timestamp, data, CancellationToken.None);
        }

        /// <summary>
        /// Sends an event to the path on the device and returns the value after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="timestamp"></param>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<byte[]> SendEvent(RnetEvent evt, ushort timestamp, ushort data, CancellationToken cancellationToken)
        {
            return SendEvent(evt, timestamp, data, RnetPriority.Low, cancellationToken);
        }

        /// <summary>
        /// Sends an event to the path on the device and returns the value after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="timestamp"></param>
        /// <param name="data"></param>
        /// <param name="priority"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<byte[]> SendEvent(RnetEvent evt, int timestamp, int data, RnetPriority priority, CancellationToken cancellationToken)
        {
            return checked(SendEvent(evt, (ushort)timestamp, (ushort)data, priority, cancellationToken));
        }

        /// <summary>
        /// Sends an event to the path on the device and returns the value after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="timestamp"></param>
        /// <param name="data"></param>
        /// <param name="priority"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<byte[]> SendEvent(RnetEvent evt, byte timestamp, byte data, RnetPriority priority, CancellationToken cancellationToken)
        {
            return SendEvent(evt, (ushort)timestamp, (ushort)data, priority, cancellationToken);
        }

        /// <summary>
        /// Sends an event to the path on the device and returns the value after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="timestamp"></param>
        /// <param name="data"></param>
        /// <param name="priority"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task<byte[]> SendEvent(RnetEvent evt, ushort timestamp, ushort data, RnetPriority priority, CancellationToken cancellationToken);

        /// <summary>
        /// Raised when the data buffer stored in the node is changed.
        /// </summary>
        public event EventHandler<RnetDataAvailableEventArgs> DataAvailable;

        /// <summary>
        /// Raises the BufferChanged event.
        /// </summary>
        /// <param name="args"></param>
        protected void RaiseDataAvailable(RnetDataAvailableEventArgs args)
        {
            if (DataAvailable != null)
                DataAvailable(this, args);
        }

        /// <summary>
        /// Implements <see cref="IObserver<byte[]>"/> so the subscriptions to data modifications can be monitored
        /// without events.
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        IDisposable IObservable<byte[]>.Subscribe(IObserver<byte[]> observer)
        {
            return Observable.FromEventPattern<RnetDataAvailableEventArgs>(
                    h => DataAvailable += h,
                    h => DataAvailable -= h)
                .Select(i => i.EventArgs.Data)
                .Merge(Current != null ? Observable.Return(Current) : Observable.Empty<byte[]>())
                .Subscribe(observer);
        }

    }

}
