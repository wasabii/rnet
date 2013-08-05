using System;
using System.Threading;
using System.Threading.Tasks;

using Nito.AsyncEx;

namespace Rnet
{

    /// <summary>
    /// Provides a handle by which to manipulate data at a specific path in an RNET device.
    /// </summary>
    public class RnetDataHandle
    {

        AsyncMonitor wait = new AsyncMonitor();
        byte[] buffer;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="parent"></param>
        /// <param name="path"></param>
        internal RnetDataHandle(RnetDevice device, RnetPath path)
        {
            if (device == null)
                throw new ArgumentNullException("device");

            Device = device;
            Path = path;
        }

        /// <summary>
        /// Device that owns this handle.
        /// </summary>
        public RnetDevice Device { get; private set; }

        /// <summary>
        /// Full path of this handle.
        /// </summary>
        public RnetPath Path { get; private set; }

        /// <summary>
        /// Invoked by the device when data has been received. Makes the data available to users of this handle instance.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal async Task Receive(byte[] data)
        {
            using (await wait.EnterAsync())
            {
                // check for duplicates; unneccessary events might be hard on consumers due to parsing
                if (data.ArrayEquals(buffer))
                    return;

                // store data locally
                buffer = data;

                // notify anything waiting on the data
                wait.PulseAll();
            }

            // send new data to interested parties
            if (data != null)
                RaiseDataAvailable(new RnetDataAvailableEventArgs(data));
        }

        /// <summary>
        /// Reads the data from the device path. Cached data may be returned if available.
        /// </summary>
        /// <returns></returns>
        public Task<byte[]> Read()
        {
            return Read(CancellationToken.None);
        }

        /// <summary>
        /// Reads the data from the device path. Cached data may be returned if available.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<byte[]> Read(CancellationToken cancellationToken)
        {
            return await RnetUtil.DefaultIfCancelled(async ct =>
            {
                using (await wait.EnterAsync(ct))
                {
                    // cache data available
                    if (buffer != null)
                        return buffer;

                    // issue request for data
                    await Device.SendRequestData(Path, ct);

                    // wait for data to arrive
                    while (buffer == null && !ct.IsCancellationRequested)
                        await wait.WaitAsync(ct);

                    return buffer;
                }
            }, cancellationToken, Device.ReadTimeoutCancellationToken);
        }

        /// <summary>
        /// Expires any cached data.
        /// </summary>
        /// <returns></returns>
        public Task<byte[]> Refresh()
        {
            return Refresh(CancellationToken.None);
        }

        /// <summary>
        /// Expires any cached data.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<byte[]> Refresh(CancellationToken cancellationToken)
        {
            return await RnetUtil.DefaultIfCancelled(async ct =>
            {
                // expire cached data
                using (await wait.EnterAsync(ct))
                    buffer = null;

                // reads the new data
                return await Read(ct);
            }, cancellationToken, Device.ReadTimeoutCancellationToken);
        }

        /// <summary>
        /// Writes the data to the device and returns the new value.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task<byte[]> Write(byte[] data)
        {
            return Write(data, CancellationToken.None);
        }

        /// <summary>
        /// Writes the data to the device and returns the new value.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<byte[]> Write(byte[] data, CancellationToken cancellationToken)
        {
            return await RnetUtil.DefaultIfCancelled(async ct =>
            {
                using (await wait.EnterAsync(ct))
                {
                    // clear cached buffer, will reread
                    buffer = null;

                    // initiate set data message followed by request data message
                    await Device.SendSetData(Path, data, ct);
                    await Device.SendRequestData(Path, ct);

                    // wait for new data value
                    while (buffer == null && !ct.IsCancellationRequested)
                        await wait.WaitAsync(ct);

                    return buffer;
                }
            }, cancellationToken, Device.WriteTimeoutCancellationToken);
        }

        /// <summary>
        /// Writes the single byte to the path and returns the new value. The new value may be different than what was written.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Task<byte[]> Write(byte value)
        {
            return Write(value, CancellationToken.None);
        }

        /// <summary>
        /// Writes the single byte to the path and returns the new value. The new value may be different than what was written.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<byte[]> Write(byte value, CancellationToken cancellationToken)
        {
            return Write(new[] { value }, cancellationToken);
        }

        /// <summary>
        /// Sends an event to the path and returns the value after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        public Task<byte[]> SendEvent(RnetEvent evt)
        {
            return SendEvent(evt, (ushort)0, CancellationToken.None);
        }

        /// <summary>
        /// Sends an event to the path and returns the value after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<byte[]> SendEvent(RnetEvent evt, CancellationToken cancellationToken)
        {
            return SendEvent(evt, (ushort)0, cancellationToken);
        }

        /// <summary>
        /// Sends an event to the path and returns the value after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public Task<byte[]> SendEvent(RnetEvent evt, ushort timestamp)
        {
            return SendEvent(evt, timestamp, (ushort)0, CancellationToken.None);
        }

        /// <summary>
        /// Sends an event to the path and returns the value after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public Task<byte[]> SendEvent(RnetEvent evt, int timestamp)
        {
            return checked(SendEvent(evt, (ushort)timestamp));
        }

        /// <summary>
        /// Sends an event to the path and returns the value after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public Task<byte[]> SendEvent(RnetEvent evt, byte timestamp)
        {
            return SendEvent(evt, (ushort)timestamp);
        }

        /// <summary>
        /// Sends an event to the path and returns the value after the event was received.
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
        /// Sends an event to the path and returns the value after the event was received.
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
        /// Sends an event to the path and returns the value after the event was received.
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
        /// Sends an event to the path and returns the value after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="timestamp"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task<byte[]> SendEvent(RnetEvent evt, ushort timestamp, ushort data)
        {
            return SendEvent(evt, timestamp, data, RnetPriority.Low, CancellationToken.None);
        }

        /// <summary>
        /// Sends an event to the path and returns the value after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="timestamp"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task<byte[]> SendEvent(RnetEvent evt, int timestamp, int data)
        {
            return checked(SendEvent(evt, (ushort)timestamp, (ushort)data));
        }

        /// <summary>
        /// Sends an event to the path and returns the value after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="timestamp"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task<byte[]> SendEvent(RnetEvent evt, byte timestamp, byte data)
        {
            return SendEvent(evt, (ushort)timestamp, (ushort)data);
        }

        /// <summary>
        /// Sends an event to the path and returns the value after the event was received.
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
        /// Sends an event to the path and returns the value after the event was received.
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
        /// Sends an event to the path and returns the value after the event was received.
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
        /// Sends an event to the path and returns the value after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="timestamp"></param>
        /// <param name="data"></param>
        /// <param name="priority"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<byte[]> SendEvent(RnetEvent evt, ushort timestamp, ushort data, RnetPriority priority, CancellationToken cancellationToken)
        {
            return await RnetUtil.DefaultIfCancelled(async ct =>
            {
                using (await wait.EnterAsync(ct))
                {
                    // clear cached data
                    buffer = null;

                    // initiate event followed by request data message
                    await Device.SendEvent(Path, evt, timestamp, data, priority, ct);
                    await Device.SendRequestData(Path, ct);

                    // wait for new data value
                    while (buffer == null && !ct.IsCancellationRequested)
                        await wait.WaitAsync(ct);

                    return buffer;
                }
            }, cancellationToken, Device.EventTimeoutCancellationToken);
        }

        /// <summary>
        /// Sends an event to the path and returns the value after the event was received.
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
        /// Sends an event to the path and returns the value after the event was received.
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
        /// Raised when the data buffer stored in the node is changed.
        /// </summary>
        public event EventHandler<RnetDataAvailableEventArgs> DataAvailable;

        /// <summary>
        /// Raises the BufferChanged event.
        /// </summary>
        /// <param name="args"></param>
        void RaiseDataAvailable(RnetDataAvailableEventArgs args)
        {
            if (DataAvailable != null)
                DataAvailable(this, args);
        }

    }

}
