using System.Threading;
using System.Threading.Tasks;

using Nito.AsyncEx;

namespace Rnet
{

    /// <summary>
    /// Provides a handle by which to manipulate data at a specific path in a remote RNET device.
    /// </summary>
    public sealed class RnetRemoteDataHandle : RnetDataHandle
    {

        AsyncMonitor wait = new AsyncMonitor();
        byte[] buffer;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="path"></param>
        internal RnetRemoteDataHandle(RnetRemoteDevice device, RnetPath path)
            : base(device, path)
        {

        }

        /// <summary>
        /// Owning remote device of the handle.
        /// </summary>
        public new RnetRemoteDevice Device
        {
            get { return (RnetRemoteDevice)base.Device; }
        }

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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<byte[]> Read(CancellationToken cancellationToken)
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
        /// Refreshes the data from the device.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<byte[]> Refresh(CancellationToken cancellationToken)
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
        /// Writes the byte to the device and returns the new value. The new value may be different from what was
        /// written.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<byte[]> Write(byte[] data, CancellationToken cancellationToken)
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
        /// Sends an event to the path on the device and returns the value after the event was received.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="timestamp"></param>
        /// <param name="data"></param>
        /// <param name="priority"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<byte[]> SendEvent(RnetEvent evt, ushort timestamp, ushort data, RnetPriority priority, CancellationToken cancellationToken)
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

    }

}
