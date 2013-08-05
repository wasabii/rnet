using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using Nito.AsyncEx;

namespace Rnet
{

    /// <summary>
    /// Base class of RNET devices.
    /// </summary>
    public abstract class RnetDevice : RnetBusObject
    {

        const double DEFAULT_RETRY_DELAY_SECONDS = 1;
        const double DEFAULT_TIMEOUT_SECONDS = 60;

        AsyncLock send = new AsyncLock();
        AsyncLock receive = new AsyncLock();
        AsyncMonitor handshake = new AsyncMonitor();
        RnetHandshakeMessage handshakeMessage;

        ConcurrentDictionary<RnetPath, WeakReference<RnetDataHandle>> handles =
            new ConcurrentDictionary<RnetPath, WeakReference<RnetDataHandle>>();
        ConcurrentDictionary<RnetPath, WeakReference<RnetDataHandleWriter>> writers =
            new ConcurrentDictionary<RnetPath, WeakReference<RnetDataHandleWriter>>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected RnetDevice(RnetBus bus)
        {
            Bus = bus;
            Visible = false;
            RequiresHandshake = true;
            RetryDelay = TimeSpan.FromSeconds(DEFAULT_RETRY_DELAY_SECONDS);
            RequestTimeout = TimeSpan.FromSeconds(DEFAULT_TIMEOUT_SECONDS);
            SetDataTimeout = TimeSpan.FromSeconds(DEFAULT_TIMEOUT_SECONDS);
            EventTimeout = TimeSpan.FromSeconds(DEFAULT_TIMEOUT_SECONDS);
        }

        /// <summary>
        /// Reference to the communications bus.
        /// </summary>
        public RnetBus Bus { get; protected set; }

        /// <summary>
        /// Gets the ID of the device on the RNET bus.
        /// </summary>
        public abstract RnetDeviceId DeviceId { get; }

        /// <summary>
        /// Set to <c>true</c> during device discovery.
        /// </summary>
        public bool Visible { get; internal set; }

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
        public TimeSpan RequestTimeout { get; set; }

        /// <summary>
        /// Gets or sets the amount of time to wait for a handshake for a set data message.
        /// </summary>
        public TimeSpan SetDataTimeout { get; set; }

        /// <summary>
        /// Gets or sets the amount of time to wait for a handshake for a set data message.
        /// </summary>
        public TimeSpan EventTimeout { get; set; }

        /// <summary>
        /// Gets the default <see cref="CancellationToken"/> for a request data message.
        /// </summary>
        internal CancellationToken RequestDataTimeoutCancellationToken
        {
            get { return new CancellationTokenSource(RequestTimeout).Token; }
        }

        /// <summary>
        /// Gets the default <see cref="CancellationToken"/> for a set data message.
        /// </summary>
        internal CancellationToken SetDataTimeoutCancellationToken
        {
            get { return new CancellationTokenSource(SetDataTimeout).Token; }
        }

        /// <summary>
        /// Gets the default <see cref="CancellationToken"/> for an event message.
        /// </summary>
        internal CancellationToken EventTimeoutCancellationToken
        {
            get { return new CancellationTokenSource(EventTimeout).Token; }
        }

        /// <summary>
        /// Bus has received a message from the device.
        /// </summary>
        /// <param name="message"></param>
        internal async Task ReceiveMessage(RnetMessage message)
        {
            using (await receive.LockAsync())
            {
                var hmsg = message as RnetHandshakeMessage;
                if (hmsg != null)
                    await ReceiveHandshakeMessage(hmsg);

                var dmsg = message as RnetSetDataMessage;
                if (dmsg != null)
                    await ReceiveSetDataMessage(dmsg);

                var emsg = message as RnetEventMessage;
                if (emsg != null)
                    await ReceiveEventMessage(emsg);
            }
        }

        /// <summary>
        /// Device has sent us a handshake message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        async Task ReceiveHandshakeMessage(RnetHandshakeMessage message)
        {
            using (await handshake.EnterAsync())
            {
                handshakeMessage = message;
                handshake.PulseAll();
            }
        }

        /// <summary>
        /// Device has sent us a set data message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        async Task ReceiveSetDataMessage(RnetSetDataMessage message)
        {
            // device requires handshake
            if (RequiresHandshake)
                await Bus.Client.SendAsync(new RnetHandshakeMessage(
                    message.SourceDeviceId,
                    Bus.Device != null ? Bus.Device.DeviceId : RnetDeviceId.External,
                    RnetHandshakeType.Data));

            // create new data buffer if packet is first in set
            if (message.PacketNumber == 0)
                writers.Remove(message.SourcePath);

            // obtain data writer
            var writer = GetPathDataWriter(message.SourcePath, message.PacketCount);

            // write message to writer
            writer.Write(message.Data.ToArray(), message.PacketNumber);

            // end of message stream, receive incoming data
            if (writer.IsComplete)
                await ReceiveData(message.SourcePath, writer.ToArray());
        }

        /// <summary>
        /// Device has sent us an event message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        async Task ReceiveEventMessage(RnetEventMessage message)
        {
            if (message.Priority == RnetPriority.High)
                await Bus.Client.SendAsync(new RnetHandshakeMessage(
                    message.SourceDeviceId,
                    Bus.Device != null ? Bus.Device.DeviceId : RnetDeviceId.External,
                    RnetHandshakeType.Event));
        }

        /// <summary>
        /// Invoked when a set data message arrives with data from this device. Integrates the data into the path
        /// handle.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        async Task ReceiveData(RnetPath path, byte[] data)
        {
            await GetPathHandle(path).Receive(data);
        }

        /// <summary>
        /// Issues a request data message to the device.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal async Task SendRequestData(RnetPath path, CancellationToken cancellationToken)
        {
            // only one request at a time
            using (await send.LockAsync(cancellationToken))
                await Bus.Client.SendAsync(new RnetRequestDataMessage(
                    DeviceId,
                    Bus.Device != null ? Bus.Device.DeviceId : RnetDeviceId.External,
                    path,
                    RnetPath.Empty,
                    RnetRequestMessageType.Data));
        }

        /// <summary>
        /// Issues a set data message to the device.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal async Task SendSetData(RnetPath path, byte[] data, CancellationToken cancellationToken)
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
                        await Bus.Client.SendAsync(new RnetSetDataMessage(
                            DeviceId,
                            Bus.Device.DeviceId,
                            path,
                            RnetPath.Empty,
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
                await Bus.Client.SendAsync(new RnetEventMessage(
                    DeviceId,
                    Bus.Device.DeviceId,
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
        /// Gets a handle to the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public RnetDataHandle this[RnetPath path]
        {
            get { return GetPathHandle(path); }
        }

        /// <summary>
        /// Gets a handle to the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public RnetDataHandle this[byte a]
        {
            get { return GetPathHandle(new RnetPath(a)); }
        }

        /// <summary>
        /// Gets a handle to the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public RnetDataHandle this[byte a, byte b]
        {
            get { return GetPathHandle(new RnetPath(a, b)); }
        }

        /// <summary>
        /// Gets a handle to the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public RnetDataHandle this[byte a, byte b, byte c]
        {
            get { return GetPathHandle(new RnetPath(a, b, c)); }
        }

        /// <summary>
        /// Gets a handle to the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public RnetDataHandle this[byte a, byte b, byte c, byte d]
        {
            get { return GetPathHandle(new RnetPath(a, b, c, d)); }
        }

        /// <summary>
        /// Gets a handle to the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public RnetDataHandle this[byte a, byte b, byte c, byte d, byte e]
        {
            get { return GetPathHandle(new RnetPath(a, b, c, d, e)); }
        }

        /// <summary>
        /// Gets a handle to the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public RnetDataHandle this[byte a, byte b, byte c, byte d, byte e, byte f]
        {
            get { return GetPathHandle(new RnetPath(a, b, c, d, e, f)); }
        }

        /// <summary>
        /// Gets a handle to the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public RnetDataHandle this[byte a, byte b, byte c, byte d, byte e, byte f, byte g]
        {
            get { return GetPathHandle(new RnetPath(a, b, c, d, e, f, g)); }
        }

        /// <summary>
        /// Gets a handle to the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public RnetDataHandle this[byte a, byte b, byte c, byte d, byte e, byte f, byte g, byte h]
        {
            get { return GetPathHandle(new RnetPath(a, b, c, d, e, f, g, h)); }
        }

        /// <summary>
        /// Gets a handle to the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        RnetDataHandle GetPathHandle(RnetPath path)
        {
            return handles
                .GetOrAdd(path, i => new WeakReference<RnetDataHandle>(new RnetDataHandle(this, i)))
                .GetTargetOrDefault();
        }

        /// <summary>
        /// Gets a handle to the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="packetCount"></param>
        /// <returns></returns>
        RnetDataHandleWriter GetPathDataWriter(RnetPath path, int packetCount)
        {
            return writers
                .GetOrAdd(path, i => new WeakReference<RnetDataHandleWriter>(new RnetDataHandleWriter(packetCount)))
                .GetTargetOrDefault();
        }

    }

}
