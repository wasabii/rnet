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

        internal AsyncLock asyncLock = new AsyncLock();
        AsyncMonitor asyncMonitor = new AsyncMonitor();
        string model;
        Dictionary<RnetPath, RnetDeviceDataBuffer> buffers;
        RnetHandshakeMessage handshake;
        RnetDeviceDirectoryRoot directory;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        RnetDevice()
        {
            buffers = new Dictionary<RnetPath, RnetDeviceDataBuffer>();
            directory = new RnetDeviceDirectoryRoot(this);
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
            RequestTimeout = TimeSpan.FromSeconds(2.5);
            SetDataTimeout = TimeSpan.FromSeconds(2.5);
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
        /// Model name of the device.
        /// </summary>
        public string Model
        {
            get { return model; }
            set { model = value; RaisePropertyChanged("Model"); RaisePropertyChanged("Name"); }
        }

        /// <summary>
        /// Gets the root of the device data directory tree.
        /// </summary>
        public RnetDeviceDirectoryRoot Directory
        {
            get { return directory; }
        }

        /// <summary>
        /// Bus has received a message from the device.
        /// </summary>
        /// <param name="message"></param>
        internal Task ReceiveMessage(RnetMessage message)
        {
            var dmsg = message as RnetSetDataMessage;
            if (dmsg != null)
                return ReceiveSetDataMessage(dmsg);

            return Task.FromResult(true);
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
                Bus.Client.SendMessage(new RnetHandshakeMessage(message.SourceDeviceId, DeviceId,
                    RnetHandshakeType.Data),
                    RnetMessagePriority.High);

            // get or add data item if first packet
            var data = buffers.GetOrDefault(message.SourcePath);
            if (data == null)
                if (message.PacketNumber == 0)
                    data = buffers[message.SourcePath] = new RnetDeviceDataBuffer();

            // no data item could be created, perhaps out of order packet
            if (data == null)
                return;

            // receive data
            data.Write(message.Data.ToArray(), message.PacketNumber);

            // end of packet stream
            if (data.IsComplete)
                await directory.SetAsync(message.SourcePath, data.ToArray());
        }

        /// <summary>
        /// Reads the data from the device at the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal async Task<RnetDeviceDirectory> RequestAsync(RnetPath path, CancellationToken cancellationToken)
        {
            using (await asyncMonitor.EnterAsync(cancellationToken))
            {
                Bus.SendRequestDataMessage(DeviceId, path, RnetPath.Empty);
                return await directory.WaitAsync(path, cancellationToken);
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
            using (await asyncMonitor.EnterAsync())
            {
                var c = buffer.Length / 16;

                // send in 16 byte chunks
                for (ushort n = 0; n < c / 16; n++)
                {
                    // copy packet data
                    var s = (ushort)Math.Min(16, buffer.Length % 16);
                    var d = new byte[s];
                    Array.Copy(buffer, d, s);

                    // clear received handshake
                    handshake = null;

                    // send set data packet
                    Bus.Client.SendMessage(new RnetSetDataMessage(DeviceId, Bus.ClientDevice.DeviceId,
                        path,
                        RnetPath.Empty,
                        n,
                        s,
                        new RnetData(d)));

                    // wait for handshake
                    while (handshake == null)
                        await asyncMonitor.WaitAsync(cancellationToken);
                }
            }
        }

        /// <summary>
        /// Gets the display name of the device.
        /// </summary>
        public override string Name
        {
            get { return GetDisplayName(); }
        }

        /// <summary>
        /// Implements DisplayName.
        /// </summary>
        /// <returns></returns>
        public string GetDisplayName()
        {
            if (string.IsNullOrWhiteSpace(Model))
                return string.Format("({0}.{1}.{2})", (byte)DeviceId.ControllerId, (byte)DeviceId.ZoneId, (byte)DeviceId.KeypadId);
            else
                return string.Format("{0} ({1}.{2}.{3})", Model, (byte)DeviceId.ControllerId, (byte)DeviceId.ZoneId, (byte)DeviceId.KeypadId);
        }

    }

}
