using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using Nito.AsyncEx;

namespace Rnet
{

    /// <summary>
    /// Base class of RNET devices.
    /// </summary>
    public abstract class RnetDevice : RnetModelObject
    {

        AsyncLock asyncLock = new AsyncLock();
        AsyncCollection<RnetMessage> messages = new AsyncCollection<RnetMessage>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        RnetDevice(RnetDeviceId id)
        {
            Id = id;
            DataItems = new RnetDataItemCollection(this);
            DataItemsTree = new RnetDataTreeRoot(this);
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
        /// Gets the set of data items stored in this device.
        /// </summary>
        public RnetDataItemCollection DataItems { get; private set; }

        /// <summary>
        /// Gets the tree organized set of data items stored in this device.
        /// </summary>
        public RnetDataTreeNode DataItemsTree { get; private set; }

        /// <summary>
        /// Bus has received a message from the device.
        /// </summary>
        /// <param name="message"></param>
        internal Task ReceiveMessage(RnetMessage message)
        {
            return messages.AddAsync(message);
        }

        /// <summary>
        /// Initiates a request for the data at the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal async Task<RnetDataItem> RequestDataItem(RnetPath path, CancellationToken cancellationToken)
        {
            // request message stream for path
            var l = await RequestData(path, cancellationToken);
            if (l == null)
                return null;

            // first message in stream
            var m = l.FirstOrDefault();
            if (m == null)
                return null;

            // write packets to new data item
            var d = new RnetDataItem(path);
            d.WriteBegin(m.PacketCount);
            foreach (var i in l)
                d.Write(i.Data.ToArray(), i.PacketNumber);
            d.WriteEnd();
            return d;
        }

        /// <summary>
        /// Submits the given request message and waits for a valid reply.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="handleReply"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        internal async Task<T> RequestReply<T>(
            RnetMessage message,
            Func<RnetMessage, T> handleReply,
            CancellationToken cancellationToken,
            CancellationToken timeoutCancellationToken)
            where T : class
        {
            if (message == null)
                throw new ArgumentNullException("message");

            using (await asyncLock.LockAsync())
            {
                // send initial conversation message
                Bus.Client.SendMessage(message);

                // time between retries
                var retryTimeoutCancellationToken = new CancellationTokenSource(RetryDelay).Token;

                // wait for reply
                while (handleReply != null)
                {
                    // retry initial packet if retry time has elapsed
                    if (message != null)
                        if (retryTimeoutCancellationToken.IsCancellationRequested)
                            Bus.Client.SendMessage(message);

                    // time between retries
                    if (retryTimeoutCancellationToken.IsCancellationRequested)
                        retryTimeoutCancellationToken = new CancellationTokenSource(RetryDelay).Token;

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
                            retryTimeoutCancellationToken).Token;

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
        /// Initiates a request for data for the specified device and path.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="targetPath"></param>
        internal async Task<IEnumerable<RnetSetDataMessage>> RequestData(RnetPath targetPath, CancellationToken cancellationToken)
        {
            RnetSetDataMessage[] msgs = null;

            // add a conversation that sends a request data message and expects a set data message in return
            return await RequestReply(new RnetRequestDataMessage(Id, Bus.Id,
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
        /// Returns a string representation of the current instance.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}/{1}/{2}", Id.ControllerId, Id.ZoneId, Id.KeypadId);
        }

    }

}
