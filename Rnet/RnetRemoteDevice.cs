using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

using Nito.AsyncEx;

namespace Rnet
{

    /// <summary>
    /// Reference to an RNET device present on the RNET bus.
    /// </summary>
    public abstract class RnetRemoteDevice : RnetDevice
    {

        const double DEFAULT_RETRY_DELAY_SECONDS = 1;
        const double DEFAULT_TIMEOUT_SECONDS = 3;

        AsyncLock send = new AsyncLock();
        AsyncLock receive = new AsyncLock();
        AsyncMonitor handshake = new AsyncMonitor();
        RnetHandshakeMessage handshakeMessage;

        ConcurrentDictionary<RnetPath, WeakReference<RnetDataHandleWriter>> writers =
            new ConcurrentDictionary<RnetPath, WeakReference<RnetDataHandleWriter>>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        internal protected RnetRemoteDevice(RnetBus bus)
            : base(bus)
        {
            RequiresHandshake = true;
            RetryDelay = TimeSpan.FromSeconds(DEFAULT_RETRY_DELAY_SECONDS);
            ReadTimeout = TimeSpan.FromSeconds(DEFAULT_TIMEOUT_SECONDS);
            WriteTimeout = TimeSpan.FromSeconds(DEFAULT_TIMEOUT_SECONDS);
            EventTimeout = TimeSpan.FromSeconds(DEFAULT_TIMEOUT_SECONDS);
        }

        /// <summary>
        /// Gets whether or not this device requires a handshake in response to high priority messages.
        /// </summary>
        public bool RequiresHandshake { get; set; }

        /// <summary>
        /// Gets the time between retries.
        /// </summary>
        public TimeSpan RetryDelay { get; set; }

        /// <summary>
        /// Gets or sets the amount of time to wait for a response to a request message.
        /// </summary>
        public TimeSpan ReadTimeout { get; set; }

        /// <summary>
        /// Gets or sets the amount of time to wait for a handshake for a set data message.
        /// </summary>
        public TimeSpan WriteTimeout { get; set; }

        /// <summary>
        /// Gets or sets the amount of time to wait for a handshake for a set data message.
        /// </summary>
        public TimeSpan EventTimeout { get; set; }

        /// <summary>
        /// Gets the timeout <see cref="CancellationToken"/> for a read operation.
        /// </summary>
        internal CancellationToken ReadTimeoutCancellationToken
        {
            get { return new CancellationTokenSource(ReadTimeout).Token; }
        }

        /// <summary>
        /// Gets the timeout <see cref="CancellationToken"/> for a write operation.
        /// </summary>
        internal CancellationToken WriteTimeoutCancellationToken
        {
            get { return new CancellationTokenSource(WriteTimeout).Token; }
        }

        /// <summary>
        /// Gets the timeout <see cref="CancellationToken"/> for an event operation.
        /// </summary>
        internal CancellationToken EventTimeoutCancellationToken
        {
            get { return new CancellationTokenSource(EventTimeout).Token; }
        }

        /// <summary>
        /// Bus has received a message from the given device.
        /// </summary>
        /// <param name="message"></param>
        internal async Task SentMessage(RnetMessage message)
        {
            Contract.Requires<ArgumentNullException>(message != null);

            var hmsg = message as RnetHandshakeMessage;
            if (hmsg != null)
                await ReceiveHandshakeMessage(hmsg);

            var rmsg = message as RnetRequestDataMessage;
            if (rmsg != null)
                await ReceiveRequestDataMessage(rmsg);

            var dmsg = message as RnetSetDataMessage;
            if (dmsg != null)
                await ReceiveSetDataMessage(dmsg);

            var emsg = message as RnetEventMessage;
            if (emsg != null)
                await ReceiveEventMessage(emsg);
        }

        /// <summary>
        /// Device has sent us a handshake message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        async Task ReceiveHandshakeMessage(RnetHandshakeMessage message)
        {
            Contract.Requires<ArgumentNullException>(message != null);

            using (await handshake.EnterAsync())
            {
                handshakeMessage = message;
                handshake.PulseAll();
            }
        }

        /// <summary>
        /// Device has sent us a request data message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        async Task ReceiveRequestDataMessage(RnetRequestDataMessage message)
        {
            Contract.Requires<ArgumentNullException>(message != null);

            // read data from the local device
            var data = await Bus.LocalDevice[message.TargetPath].Read();
            if (data == null)
                return;

            // reply with the data from the requested path
            await SendSetDataReply(message.TargetPath, data, CancellationToken.None);
        }

        /// <summary>
        /// Device has sent us a set data message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        async Task ReceiveSetDataMessage(RnetSetDataMessage message)
        {
            Contract.Requires<InvalidOperationException>(Bus != null);
            Contract.Requires<ArgumentNullException>(message != null);

            using (await receive.LockAsync())
            {
                // device requires handshake
                if (RequiresHandshake)
                    await Bus.Client.Send(new RnetHandshakeMessage(
                        message.SourceDeviceId,
                        Bus.LocalDevice.DeviceId,
                        RnetHandshakeType.Data));

                // create new data buffer if packet is first in set
                if (message.PacketNumber == 0)
                    writers.Remove(message.SourcePath);

                // obtain data writer
                var writer = GetOrCreateDataHandleWriter(message.SourcePath, message.PacketCount);

                // write message to writer
                writer.Write(message.Data.ToArray(), message.PacketNumber);

                // end of message stream, receive incoming data
                if (writer.IsComplete)
                    await ReceiveData(message.SourcePath, writer.ToArray());
            }
        }

        /// <summary>
        /// Invoked when a set data message arrives with data from this device. Integrates the data into the path
        /// handle.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Task ReceiveData(RnetPath path, byte[] data)
        {
            Contract.Requires<ArgumentException>(path.Length != 0);
            Contract.Requires<ArgumentNullException>(data != null);

            return GetOrCreateDataHandle(path).Receive(data);
        }

        /// <summary>
        /// Device has sent us an event message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        async Task ReceiveEventMessage(RnetEventMessage message)
        {
            Contract.Requires<ArgumentNullException>(message != null);

            using (await receive.LockAsync())
                if (message.Priority == RnetPriority.High)
                    await Bus.Client.Send(new RnetHandshakeMessage(
                        message.SourceDeviceId,
                        Bus.LocalDevice.DeviceId,
                        RnetHandshakeType.Event));
        }

        /// <summary>
        /// Issues a request data message to the device.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal async Task SendRequestData(RnetPath path, CancellationToken cancellationToken)
        {
            Contract.Requires<ArgumentException>(path.Length != 0);

            // only one request at a time
            using (await send.LockAsync(cancellationToken))
                await Bus.Client.Send(new RnetRequestDataMessage(
                    DeviceId,
                    Bus.LocalDevice.DeviceId,
                    path,
                    RnetPath.Empty,
                    RnetRequestMessageType.Data));
        }

        /// <summary>
        /// Issues a set data message to the device.
        /// </summary>
        /// <param name="targetPath"></param>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal Task SendSetData(RnetPath targetPath, byte[] data, CancellationToken cancellationToken)
        {
            return SendSetData(targetPath, RnetPath.Empty, data, cancellationToken);
        }

        /// <summary>
        /// Issues a set data message to the device in response to a request data message.
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SendSetDataReply(RnetPath sourcePath, byte[] data, CancellationToken cancellationToken)
        {
            return SendSetData(RnetPath.Empty, sourcePath, data, cancellationToken);
        }

        /// <summary>
        /// Issues a set data message to the device.
        /// </summary>
        /// <param name="targetPath"></param>
        /// <param name="sourcePath"></param>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task SendSetData(RnetPath targetPath, RnetPath sourcePath, byte[] data, CancellationToken cancellationToken)
        {
            // only one writer at a time
            using (await send.LockAsync(cancellationToken))
            {
                // number of packets to send
                int c = data.Length / 16;
                if (data.Length % 16 > 0)
                    c++;

                // send until all data sent
                for (int i = 0; i < c; i++)
                {
                    // length of packet (remainder of data, max 16)
                    var r = data.Length - i * 16;
                    var l = r % 16 > 0 ? r % 16 : 16;
                    var d = new byte[l];
                    Array.Copy(data, i * 16, d, 0, l);

                    // set data must wait for a handshake
                    using (await handshake.EnterAsync(cancellationToken))
                    {
                        handshakeMessage = null;

                        // send set data packet
                        await Bus.Client.Send(new RnetSetDataMessage(
                            DeviceId,
                            Bus.LocalDevice.DeviceId,
                            targetPath,
                            sourcePath,
                            (byte)i,
                            (byte)c,
                            new RnetData(d)));

                        // wait for handshake
                        while (handshakeMessage == null)
                            await handshake.WaitAsync(cancellationToken);
                    }
                }
            }
        }

        /// <summary>
        /// Issues an event message to the device.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="evt"></param>
        /// <param name="timestamp"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal async Task SendEvent(RnetPath path, RnetEvent evt, ushort timestamp, ushort data, RnetPriority priority, CancellationToken cancellationToken)
        {
            using (await send.LockAsync(cancellationToken))
            using (await handshake.EnterAsync(cancellationToken))
            {
                handshakeMessage = null;

                // send set data packet
                await Bus.Client.Send(new RnetEventMessage(
                    DeviceId,
                    Bus.LocalDevice.DeviceId,
                    path,
                    RnetPath.Empty,
                    evt,
                    timestamp,
                    data,
                    priority));

                // wait for handshake
                if (priority == RnetPriority.High)
                    while (handshakeMessage == null)
                        await handshake.WaitAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Gets or creates a data handle to the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        new RnetRemoteDataHandle GetOrCreateDataHandle(RnetPath path)
        {
            Contract.Requires<ArgumentException>(path.Length > 0);

            return (RnetRemoteDataHandle)base.GetOrCreateDataHandle(path);
        }

        /// <summary>
        /// Creates a new data handle to the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected internal override RnetDataHandle CreateDataHandle(RnetPath path)
        {
            return new RnetRemoteDataHandle(this, path);
        }

        /// <summary>
        /// Gets a handle to the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="packetCount"></param>
        /// <returns></returns>
        RnetDataHandleWriter GetOrCreateDataHandleWriter(RnetPath path, int packetCount)
        {
            Contract.Requires<ArgumentException>(path.Length > 0);
            Contract.Requires<ArgumentOutOfRangeException>(packetCount > 0);

            return writers
                .GetOrAdd(path, i => new WeakReference<RnetDataHandleWriter>(new RnetDataHandleWriter(packetCount)))
                .GetTargetOrDefault();
        }

    }

}
