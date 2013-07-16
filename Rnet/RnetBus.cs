using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rnet
{

    /// <summary>
    /// Initializes a connection to the Rnet bus and assembles a model of the available devices.
    /// </summary>
    public sealed class RnetBus : RnetDevice, IDisposable
    {

        /// <summary>
        /// Represents a message to be sent, followed by a message to be matched.
        /// </summary>
        class Conversation
        {

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="predicate"></param>
            public Conversation(Func<RnetMessage> send, Func<RnetMessage, bool> predicate, Action<Conversation, RnetMessage> complete)
            {
                Timestamp = DateTime.UtcNow;
                Send = send;
                Predicate = predicate;
                Complete = complete ?? ((i, j) => { });
            }

            /// <summary>
            /// Timestamp wait was initiated.
            /// </summary>
            public DateTime Timestamp { get; set; }

            /// <summary>
            /// Returns a message to be sent.
            /// </summary>
            public Func<RnetMessage> Send { get; set; }

            /// <summary>
            /// Predicate to test incoming packets to notify the completion source.
            /// </summary>
            public Func<RnetMessage, bool> Predicate { get; set; }

            /// <summary>
            /// Notified when a packet is matched.
            /// </summary>
            public Action<Conversation, RnetMessage> Complete { get; private set; }

        }

        object sync = new object();
        RnetDeviceId id;

        /// <summary>
        /// Outstanding conversations to be had.
        /// </summary>
        BlockingCollection<Conversation> conversations;

        /// <summary>
        /// Incoming messages to be processed.
        /// </summary>
        BlockingCollection<RnetMessage> messages;

        /// <summary>
        /// Signals to the thread to terminate.
        /// </summary>
        CancellationTokenSource threadCancellationTokenSource;

        /// <summary>
        /// Manages conversations.
        /// </summary>
        Thread thread;

        /// <summary>
        /// Periodically executes tasks.
        /// </summary>
        Timer timer;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="client"></param>
        public RnetBus(RnetClient client, RnetDeviceId id, SynchronizationContext synchronizationContext)
            : base(null)
        {
            // we are our own bus
            SynchronizationContext = synchronizationContext;
            Bus = this;

            if (client == null)
                throw new ArgumentNullException("client");
            if (id.KeypadId >= 0x7c && id.KeypadId <= 0x7f)
                throw new ArgumentOutOfRangeException("id", "RnetKeypadId cannot be in a reserved range.");

            this.id = id;

            // hook ourselves up to the client
            Client = client;
            Client.MessageReceived += Client_MessageReceived;

            // initialize set of known devices, including ourselves
            Devices = new RnetDeviceCollection();
            Devices.Add(this);
            Devices.RequestDevice += Devices_RequestDevice;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="client"></param>
        public RnetBus(RnetClient client)
            : this(client, RnetDeviceId.External, SynchronizationContext.Current)
        {

        }

        public override RnetDeviceId Id
        {
            get { return id; }
        }

        /// <summary>
        /// Used to report events to the user.
        /// </summary>
        public SynchronizationContext SynchronizationContext { get; private set; }

        /// <summary>
        /// <see cref="RnetClient"/> through which the bus will communicate.
        /// </summary>
        public RnetClient Client { get; private set; }

        /// <summary>
        /// RNET devices detected on the bus.
        /// </summary>
        public RnetDeviceCollection Devices { get; private set; }

        /// <summary>
        /// Starts the RNET bus.
        /// </summary>
        public void Start()
        {
            if (Client == null)
                throw new ObjectDisposedException("Bus");

            threadCancellationTokenSource = new CancellationTokenSource();
            conversations = new BlockingCollection<Conversation>();
            messages = new BlockingCollection<RnetMessage>();
            thread = new Thread(thread_Start);
            thread.Start();

            // periodically requests a refresh of all data
            timer = new Timer(timer_Run, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30));

            Client.Start();
        }

        /// <summary>
        /// Stops the RNET bus.
        /// </summary>
        public void Stop()
        {
            if (Client == null)
                throw new ObjectDisposedException("Bus");

            Client.Stop();

            if (threadCancellationTokenSource != null)
            {
                threadCancellationTokenSource.Cancel();
                threadCancellationTokenSource = null;
            }

            if (thread != null)
            {
                thread.Join();
                thread = null;
            }

            if (conversations != null)
            {
                conversations.CompleteAdding();
                conversations = null;
            }

            if (messages != null)
            {
                messages.CompleteAdding();
                messages = null;
            }
        }

        /// <summary>
        /// Invoked when a message arrives.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void Client_MessageReceived(object sender, RnetMessageEventArgs args)
        {
            lock (sync)
            {
                // client is stopped, ignore any trailing events
                if (Client.State != RnetClientState.Started)
                    return;

                // skip messages not destined to us
                var msg = args.Message;
                if (msg.TargetDeviceId != Id &&
                    msg.TargetDeviceId != RnetDeviceId.AllDevices)
                    return;

                var dmsg = msg as RnetSetDataMessage;
                if (dmsg != null)
                    ProcessMessage(dmsg);

                if (messages != null)
                    messages.Add(msg);
            }
        }

        /// <summary>
        /// Processes an incoming <see cref="RnetSetDataMessage"/>.
        /// </summary>
        /// <param name="msg"></param>
        void ProcessMessage(RnetSetDataMessage msg)
        {
            SynchronizationContext.Post(state =>
            {
                // non-external keypad IDs require a handshake to acknowledge message
                //if (Id.KeypadId != RnetKeypadId.External)
                Client.SendMessage(new RnetHandshakeMessage(msg.SourceDeviceId, Id,
                    RnetHandshakeType.Data),
                    RnetMessagePriority.High);

                var device = Devices
                    .SingleOrDefault(i => i.Id == msg.SourceDeviceId);
                if (device == null)
                {
                    // source is a controller
                    if (msg.SourceDeviceId.ZoneId == RnetZoneId.Zone1 &&
                        msg.SourceDeviceId.KeypadId == RnetKeypadId.Controller)
                        device = new RnetController(this, msg.SourceDeviceId.ControllerId);

                    // add to set of known devices
                    if (device != null)
                        Devices.Add(device);
                }

                // only if we've discovered a device
                if (device != null)
                {
                    // initialize new write on first packet
                    if (msg.PacketNumber == 0)
                        device.DataItems.WriteBegin(msg.SourcePath, msg.PacketCount);

                    // write data of packet
                    device.DataItems.Write(msg.SourcePath, msg.Data.ToArray(), msg.PacketNumber);

                    // end write on last packet
                    if (msg.PacketNumber == msg.PacketCount - 1)
                        device.DataItems.WriteEnd(msg.SourcePath);
                }
            }, null);
        }

        void thread_Start()
        {
            var cancellationToken = threadCancellationTokenSource.Token;
            if (cancellationToken == null)
                return;

            while (!cancellationToken.IsCancellationRequested)
            {
                var conversations_ = conversations;
                if (conversations_ == null)
                    return;

                Conversation conversation = null;
                if (!conversations_.TryTake(out conversation, 1000, cancellationToken))
                    continue;

                // send initial conversation message
                var message = conversation.Send();
                if (message != null)
                    Client.SendMessage(message);

                var predicate = conversation.Predicate;
                if (predicate != null)
                {
                    RnetMessage returnMessage = null;

                    // try to send conversation start a few times
                    for (int n = 0; n < 5 && returnMessage == null; n++)
                    {
                        if (n > 0 && message != null)
                            // send initial conversation message
                            Client.SendMessage(message);

                        // cancels when thread cancels, or when timeout passes
                        var ct = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken,
                            new CancellationTokenSource(500).Token).Token;

                        // find first matching return message
                        returnMessage = IgnoreException<RnetMessage, OperationCanceledException>(() =>
                            messages.GetConsumingEnumerable(ct)
                                .Where(i => i.Timestamp >= message.Timestamp)
                                .FirstOrDefault(i => predicate(i)));
                    }

                    // complete with first matching message
                    conversation.Complete(conversation, returnMessage);
                }
                else
                    // complete with no message
                    conversation.Complete(conversation, null);

                // empty out messages
                while (messages.TryTake(out message))
                    continue;
            }
        }

        T IgnoreException<T, TException>(Func<T> func)
            where TException : Exception
        {
            try
            {
                return func();
            }
            catch (TException)
            {
                return default(T);
            }
        }

        /// <summary>
        /// Invoked periodicially.
        /// </summary>
        /// <param name="state"></param>
        void timer_Run(object state)
        {
            SynchronizationContext.Post(state2 =>
            {
                lock (sync)
                {
                    foreach (var device in Devices)
                    {
                        // remove expired items with data
                        foreach (var item in device.DataItems.ToList())
                            if (item.Buffer != null)
                                if (!item.Valid)
                                    device.DataItems.Remove(item.Path);

                        // remove devices with no items
                        if (device != this)
                            if (!device.DataItems.Any())
                                Devices.Remove(device);
                    }

                    // only continue if we're currently connected
                    if (Client.ConnectionState != RnetConnectionState.Open)
                        return;
                }
            }, null);
        }

        /// <summary>
        /// Initiates a request for data for the specified device and paths.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        internal Task<RnetSetDataMessage> RequestData(RnetDeviceId deviceId, RnetPath targetPath)
        {
            var tcs = new TaskCompletionSource<RnetSetDataMessage>();

            // add a conversation that sends a request data message and expects a set data message in return
            conversations.Add(new Conversation(() =>
                new RnetRequestDataMessage(deviceId, Id,
                    targetPath,
                    null,
                    RnetRequestMessageType.Data),
                i =>
                {
                    var msg = i as RnetSetDataMessage;
                    if (msg != null &&
                        msg.SourceDeviceId == deviceId &&
                        msg.SourcePath == targetPath)
                        return true;

                    return false;
                },
                (c, m) =>
                {
                    tcs.SetResult((RnetSetDataMessage)m);
                }));

            return tcs.Task;
        }

        /// <summary>
        /// Invoked when a subscriber is added for a device id.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void Devices_RequestDevice(object sender, ValueEventArgs<RnetDeviceId> args)
        {
            // some sort of probe message
            Client.SendMessage(new RnetRequestDataMessage(args.Value, Id,
                new RnetPath(2, 0),
                null,
                RnetRequestMessageType.Data));
        }

        /// <summary>
        /// Disposes of the instance if possible.
        /// </summary>
        public void Dispose()
        {
            if (Client != null)
            {
                Stop();
                Client.MessageReceived -= Client_MessageReceived;
                Client = null;
            }
        }

    }

}
