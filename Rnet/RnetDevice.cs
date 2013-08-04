using System;
using System.Collections.Generic;
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

        AsyncLock write = new AsyncLock();
        AsyncLock request = new AsyncLock();

        AsyncMonitor handshake = new AsyncMonitor();
        RnetHandshakeMessage handshakeMessage;

        RnetDevicePathRootNode root;
        Dictionary<RnetPath, RnetDeviceDataBuffer> buffers;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        RnetDevice()
        {
            buffers = new Dictionary<RnetPath, RnetDeviceDataBuffer>();
            root = new RnetDevicePathRootNode(this);
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected RnetDevice(RnetBus bus)
            : this()
        {
            Bus = bus;
            Visible = false;
            RequiresHandshake = true;
            RetryDelay = TimeSpan.FromSeconds(1);
            RequestTimeout = TimeSpan.FromSeconds(60);
            SetDataTimeout = TimeSpan.FromSeconds(60);
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
        /// Gets the default <see cref="CancellationToken"/> for a data request.
        /// </summary>
        public CancellationToken RequestDataCancellationToken
        {
            get { return new CancellationTokenSource(RequestTimeout).Token; }
        }

        /// <summary>
        /// Gets the default <see cref="CancellationToken"/> for a set data message.
        /// </summary>
        public CancellationToken SetDataCancellationToken
        {
            get { return new CancellationTokenSource(SetDataTimeout).Token; }
        }

        /// <summary>
        /// Gets the root of the device data node tree.
        /// </summary>
        public RnetDevicePathRootNode Root
        {
            get { return root; }
        }

        /// <summary>
        /// Bus has received a message from the device.
        /// </summary>
        /// <param name="message"></param>
        internal Task ReceiveMessage(RnetMessage message)
        {
            var hmsg = message as RnetHandshakeMessage;
            if (hmsg != null)
                return ReceiveHandshakeMessage(hmsg);

            var dmsg = message as RnetSetDataMessage;
            if (dmsg != null)
                return ReceiveSetDataMessage(dmsg);

            var emsg = message as RnetEventMessage;
            if (emsg != null)
                return ReceiveEventMessage(emsg);

            return Task.FromResult(true);
        }

        /// <summary>
        /// Bus has received a handshake message from device.
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
        /// Bus has received a set data message from device. Integrates into local data structure.
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
                buffers[message.SourcePath] = new RnetDeviceDataBuffer(message.PacketCount);

            // get obtain buffer, might fail if packet is out of order
            var data = buffers.GetOrDefault(message.SourcePath);
            if (data == null)
                return;

            // receive data
            data.Write(message.Data.ToArray(), message.PacketNumber);

            // end of packet stream
            if (data.IsComplete)
                await root.SetAsync(message.SourcePath, data.ToArray());
        }

        /// <summary>
        /// Bus has received an event message from device.
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
        /// Reads the data from the device at the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal async Task<RnetDevicePathNode> RequestAsync(RnetPath path, CancellationToken cancellationToken)
        {
            // only one request at a time
            using (await request.LockAsync(cancellationToken))
            {
                await Bus.RequestDataAsync(DeviceId, path);

                // wait for node to appear in structure
                return await root.WaitAsync(path, cancellationToken);
            }
        }

        /// <summary>
        /// Writes the data to the device at the specified path.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal async Task WriteAsync(RnetPath path, byte[] buffer, CancellationToken cancellationToken)
        {
            // only one writer at a time
            using (await write.LockAsync())
            {
                // number of packets to send
                int c = buffer.Length / 16;
                if (buffer.Length % 16 > 0)
                    c++;

                // send until all data sent
                for (int i = 0; i < c; i++)
                {
                    // length of packet (remainder of data, max 16)
                    var r = buffer.Length - i * 16;
                    var l = r % 16 > 0 ? r % 16 : 16;
                    var d = new byte[l];
                    Array.Copy(buffer, i * 16, d, 0, l);

                    // set data must wait for a handshake
                    using (await handshake.EnterAsync())
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
        /// Raises an event at the specified path in the device.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="evt"></param>
        /// <param name="timestamp"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal async Task RaiseEvent(RnetPath path, RnetEvent evt, ushort timestamp, ushort data, RnetPriority priority)
        {
            using (await write.LockAsync())
            using (await handshake.EnterAsync())
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
                        await handshake.WaitAsync();
            }
        }

    }

}
