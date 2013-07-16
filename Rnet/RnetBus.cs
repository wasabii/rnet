using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        /// My device ID.
        /// </summary>
        RnetDeviceId id;

        /// <summary>
        /// Outstanding conversations to engage in.
        /// </summary>
        BlockingCollection<Conversation> conversations;

        /// <summary>
        /// Incoming messages to be processed by the conversation thread.
        /// </summary>
        BlockingCollection<RnetMessage> conversationMessageBuffer;

        /// <summary>
        /// Signals to the bus to stop all threads.
        /// </summary>
        CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Manages conversations.
        /// </summary>
        Thread conversationThread;

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
            Devices = new RnetDeviceCollection(this);
            Devices.Add(this);
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

            cancellationTokenSource = new CancellationTokenSource();
            conversations = new BlockingCollection<Conversation>();
            conversationMessageBuffer = new BlockingCollection<RnetMessage>();
            conversationThread = new Thread(ConversationThreadStart);
            conversationThread.IsBackground = true;
            conversationThread.Start();

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

            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource = null;
            }

            if (conversationThread != null)
            {
                conversationThread.Join();
                conversationThread = null;
            }

            if (conversations != null)
            {
                conversations.CompleteAdding();
                conversations = null;
            }

            if (conversationMessageBuffer != null)
            {
                conversationMessageBuffer.CompleteAdding();
                conversationMessageBuffer = null;
            }
        }

        /// <summary>
        /// Invoked when a message arrives.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void Client_MessageReceived(object sender, RnetMessageEventArgs args)
        {
            // client is stopped, ignore any trailing events
            if (Client.State != RnetClientState.Started)
                return;

            // skip messages not destined to us
            var msg = args.Message;
            if (msg.TargetDeviceId != Id &&
                msg.TargetDeviceId != RnetDeviceId.AllDevices)
                return;

            msg

            var dmsg = msg as RnetSetDataMessage;
            if (dmsg != null)
                ProcessMessage(dmsg);

            // buffer for conversation thread to handle messages
            if (conversationMessageBuffer != null)
                conversationMessageBuffer.Add(msg);
        }

        /// <summary>
        /// Processes an incoming <see cref="RnetSetDataMessage"/>.
        /// </summary>
        /// <param name="msg"></param>
        void ProcessMessage(RnetSetDataMessage msg)
        {
            SynchronizationContext.Post(state =>
            {
                // handshake to acknowledge message
                Client.SendMessage(new RnetHandshakeMessage(msg.SourceDeviceId, Id,
                    RnetHandshakeType.Data),
                    RnetMessagePriority.High);
            }, null);
        }

        /// <summary>
        /// Dispatches conversations to the client and handles responses.
        /// </summary>
        void ConversationThreadStart()
        {
            var cancellationToken = cancellationTokenSource.Token;
            if (cancellationToken == null)
                return;

            while (!cancellationToken.IsCancellationRequested)
            {
                var conversations_ = conversations;
                if (conversations_ == null)
                    return;

                // find next conversation
                Conversation conversation = null;
                if (!conversations_.TryTake(out conversation, 15000, cancellationToken))
                    continue;

                // cancelled conversation
                if (conversation.CancellationToken.IsCancellationRequested)
                    continue;

                // send initial conversation message
                var message = conversation.GetMessage();
                if (message != null)
                    Client.SendMessage(message);

                // are we to wait for a reply message for continuing?
                object reply = null;
                if (conversation.ExpectReply)
                    for (int n = 0; n < 4 && reply == null; n++)
                    {
                        // cancelled conversation
                        if (conversation.CancellationToken.IsCancellationRequested)
                            continue;

                        // retry initial packet
                        if (n > 0 && message != null)
                            Client.SendMessage(message);

                        // wait for reply by checking incoming messages
                        reply = RnetUtils.TryCatch<RnetMessage, OperationCanceledException>(() =>
                            conversationMessageBuffer.GetConsumingEnumerable(
                                    CancellationTokenSource.CreateLinkedTokenSource(cancellationToken,
                                    new CancellationTokenSource(500).Token).Token)
                                .Where(i => i.Timestamp >= message.Timestamp)
                                .FirstOrDefault(i => conversation.GetReply(i) != null),
                            e => conversation.SetException(e));
                    }

                // complete conversation with matched message, if any
                conversation.SetComplete(reply);
            }
        }

        /// <summary>
        /// Requests the device with the given <see cref="RnetDeviceId"/>.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        internal async Task<RnetDevice> RequestDevice(RnetDeviceId deviceId, CancellationToken cancellationToken)
        {
            var msg = await RequestData(deviceId, new RnetPath(0, 0), cancellationToken);

            // source is a controller
            if (msg.SourceDeviceId.ZoneId == RnetZoneId.Zone1 &&
                msg.SourceDeviceId.KeypadId == RnetKeypadId.Controller)
                return new RnetController(this, msg.SourceDeviceId.ControllerId);

            return null;
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
