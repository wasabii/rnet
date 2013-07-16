using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Rnet
{

    /// <summary>
    /// Base class of RNET devices.
    /// </summary>
    public abstract class RnetDevice : RnetModelObject
    {

        AsyncLock asyncLock = new AsyncLock();
        string modelName;
        AsyncCollection<RnetMessage> messages = new AsyncCollection<RnetMessage>();
        RnetDeviceDataCollection items;
        RnetDeviceDirectory directories;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        RnetDevice(RnetDeviceId id)
        {
            Id = id;
            items = new RnetDeviceDataCollection(this);
            directories = new RnetDeviceDirectory(this, null, new RnetPath());
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected RnetDevice(RnetBus bus, RnetDeviceId id)
            : this(id)
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
        public RnetDeviceId Id { get; private set; }

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
        /// Model name of the device.
        /// </summary>
        public string ModelName
        {
            get { return modelName; }
            set { modelName = value; RaisePropertyChanged("ModelName"); RaisePropertyChanged("DisplayName"); }
        }

        /// <summary>
        /// Gets the display name of the device.
        /// </summary>
        public string DisplayName
        {
            get { return ToString(); }
        }

        /// <summary>
        /// Gets all of the discovered data items in the device.
        /// </summary>
        internal RnetDeviceDataCollection Data
        {
            get { return items; }
        }

        /// <summary>
        /// Gets the root of the device data directory tree.
        /// </summary>
        public RnetDeviceDirectory Directories
        {
            get { return directories; }
        }

        /// <summary>
        /// Bus has received a message from the device.
        /// </summary>
        /// <param name="message"></param>
        internal Task ReceiveMessage(RnetMessage message)
        {
            return messages.AddAsync(message);
        }

        /// <summary>
        /// Submits the given RNET message to the device and invokes the handler function once per received message
        /// until it returns a non-null value, indicating the reply has been received.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="handleReply"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeoutCancellationToken"></param>
        /// <returns></returns>
        internal async Task<T> RequestReplyAsync<T>(
            RnetMessage message,
            Func<RnetMessage, T> handleReply,
            CancellationToken cancellationToken,
            CancellationToken timeoutCancellationToken)
            where T : class
        {
            if (message == null)
                throw new ArgumentNullException("message");

            // only a single request/reply series can be ongoing per-device
            using (await asyncLock.LockAsync())
            {
                // send initial conversation message
                Bus.Client.SendMessage(message);

                // time between retries
                var retryCancellationToken = new CancellationTokenSource(RetryDelay).Token;

                // wait for reply
                while (handleReply != null)
                {
                    // retry initial packet if retry time has elapsed
                    if (retryCancellationToken.IsCancellationRequested)
                        Bus.Client.SendMessage(message);

                    // time between retries
                    if (retryCancellationToken.IsCancellationRequested)
                        retryCancellationToken = new CancellationTokenSource(RetryDelay).Token;

                    // throw for user cancellation
                    cancellationToken.ThrowIfCancellationRequested();

                    // if the global timeout has expired, return null
                    if (timeoutCancellationToken.IsCancellationRequested)
                        return null;

                    try
                    {
                        // cancel for any reason
                        var linkedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(
                            cancellationToken,
                            timeoutCancellationToken,
                            retryCancellationToken).Token;

                        // wait for new possible reply message
                        var m = await messages.TakeAsync(linkedCancellationToken);
                        if (m == null)
                            continue;

                        // check message timestamp
                        if (m.Timestamp < message.Timestamp)
                            continue;

                        // is this message a valid reply?
                        var reply = handleReply(m);
                        if (reply != null)
                            return reply;
                    }
                    catch (OperationCanceledException)
                    {
                        // ignore
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets a stream of SetData messages for the specified path from the device.
        /// </summary>
        /// <param name="targetPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal async Task<IEnumerable<RnetSetDataMessage>> RequestDataStreamAsync(RnetPath targetPath, CancellationToken cancellationToken)
        {
            RnetSetDataMessage[] msgs = null;

            // add a conversation that sends a request data message and expects a set data message in return
            return await RequestReplyAsync(new RnetRequestDataMessage(Id, Bus.ClientDevice.Id,
                targetPath,
                null,
                RnetRequestMessageType.Data),
                i =>
                {
                    // expected set data message
                    var msg = i as RnetSetDataMessage;
                    if (msg != null &&
                        msg.SourcePath == targetPath)
                    {
                        // device requires handshake
                        if (RequiresHandshake)
                            Bus.Client.SendMessage(new RnetHandshakeMessage(msg.SourceDeviceId, Id,
                                RnetHandshakeType.Data),
                                RnetMessagePriority.High);

                        // initialize output array now that we know the size
                        if (msgs == null)
                            msgs = new RnetSetDataMessage[msg.PacketCount];

                        // insert packet into output
                        msgs[msg.PacketNumber] = msg;

                        // stream is finished
                        if (msg.PacketNumber == msg.PacketCount - 1)
                            return msgs;
                    }

                    return null;
                }, cancellationToken, new CancellationTokenSource(RequestTimeout).Token);
        }

        /// <summary>
        /// Gets the <see cref="RnetDeviceData"/> at the specified path by querying the device.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal async Task<RnetDeviceData> RequestDataAsync(RnetDeviceData data, CancellationToken cancellationToken)
        {
            // request message stream for path
            var l = await RequestDataStreamAsync(data.Path, cancellationToken);
            if (l == null)
                return null;

            // first message in stream
            var m = l.FirstOrDefault();
            if (m == null)
                return null;

            // write packets to new data item
            data.WriteBegin(m.PacketCount);
            foreach (var i in l)
                data.Write(i.Data.ToArray(), i.PacketNumber);
            data.WriteEnd();

            return data;
        }

        /// <summary>
        /// Sets the data on the specified item at the device, then initiates a refresh.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal async Task<RnetDeviceData> SetDataAsync(RnetDeviceData data, CancellationToken cancellationToken)
        {
            // add a conversation that sends a request data message and expects a set data message in return
            return await RequestReplyAsync(new RnetSetDataMessage(Id, Bus.ClientDevice.Id,
                data.Path,
                null,
                0,
                1,
                new RnetData(data.Buffer)),
                i =>
                {
                    // expected handshake message
                    var msg = i as RnetHandshakeMessage;
                    if (msg != null)
                        return data;

                    return null;
                }, cancellationToken, new CancellationTokenSource(RequestTimeout).Token);
        }

        /// <summary>
        /// Returns a string representation of the current instance.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(ModelName))
                return string.Format("({0}.{1}.{2})", (byte)Id.ControllerId, (byte)Id.ZoneId, (byte)Id.KeypadId);
            else
                return string.Format("{0} ({1}.{2}.{3})", ModelName, (byte)Id.ControllerId, (byte)Id.ZoneId, (byte)Id.KeypadId);
        }

    }

}
