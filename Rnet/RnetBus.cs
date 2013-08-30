using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

using Nito.AsyncEx;

namespace Rnet
{

    /// <summary>
    /// Provides a durable view of the state of an RNET system.
    /// </summary>
    public sealed class RnetBus : IDisposable
    {

        /// <summary>
        /// Initializes the static instance.
        /// </summary>
        static RnetBus()
        {
            RnetUriParser.RegisterParsers();
        }

        AsyncLock lck = new AsyncLock();
        RnetBusState state;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="uri"></param>
        public RnetBus(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            Uri = uri;
            Controllers = new RnetControllerCollection(this);

            // start new client
            Client = new RnetClient(Uri);
            Client.StateChanged += Client_StateChanged;
            Client.MessageReceived += Client_MessageReceived;
            Client.MessageSent += Client_MessageSent;
            Client.UnhandledException += Client_UnhandledException;
            Client.Connection.StateChanged += Connection_StateChanged;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="uri"></param>
        public RnetBus(string uri)
            : this(new Uri(uri))
        {

        }

        /// <summary>
        /// Gets the <see cref="Uri"/> by which to establish the RNET connection.
        /// </summary>
        public Uri Uri { get; private set; }

        /// <summary>
        /// Used to report events to the user.
        /// </summary>
        public SynchronizationContext SynchronizationContext { get; private set; }

        /// <summary>
        /// <see cref="RnetClient"/> through which the bus will communicate.
        /// </summary>
        public RnetClient Client { get; private set; }

        /// <summary>
        /// Local device implementation which is exposed on the bus and from which messages originate.
        /// </summary>
        public RnetLocalDevice LocalDevice { get; private set; }

        /// <summary>
        /// Controllers detected on the bus.
        /// </summary>
        public RnetControllerCollection Controllers { get; private set; }

        /// <summary>
        /// Gets the current state of the bus.
        /// </summary>
        public RnetBusState State { get; private set; }

        /// <summary>
        /// Sets the current state.
        /// </summary>
        /// <param name="state"></param>
        void SetState(RnetBusState state)
        {
            OnStateChanged(new RnetBusStateEventArgs(State = state));
        }

        /// <summary>
        /// Updates the bus state given the connection state.
        /// </summary>
        void UpdateState()
        {
            // bus has no desire to be started
            if (state != RnetBusState.Started)
                return;

            // if the connection is down, we must be trying to reconnect
            if (Client.Connection.State == RnetConnectionState.Closed)
                SetState(RnetBusState.Reconnecting);
            else
                SetState(RnetBusState.Started);
        }

        /// <summary>
        /// Starts the RNET bus.
        /// </summary>
        /// <returns></returns>
        public Task Start()
        {
            return Start(SynchronizationContext.Current, RnetKeypadId.External, CancellationToken.None);
        }

        /// <summary>
        /// Starts the RNET bus.
        /// </summary>
        /// <param name="synchronizationContext"></param>
        /// <returns></returns>
        public Task Start(SynchronizationContext synchronizationContext)
        {
            Contract.Requires<ArgumentNullException>(synchronizationContext != null);

            return Start(synchronizationContext, RnetKeypadId.External, CancellationToken.None);
        }

        /// <summary>
        /// Starts the RNET bus.
        /// </summary>
        /// <param name="synchronizationContext"></param>
        /// <param name="keypadId"></param>
        /// <returns></returns>
        public Task Start(SynchronizationContext synchronizationContext, RnetKeypadId keypadId)
        {
            return Start(SynchronizationContext.Current, keypadId, CancellationToken.None);
        }

        /// <summary>
        /// Starts the RNET bus.
        /// </summary>
        /// <param name="keypadId"></param>
        /// <returns></returns>
        public Task Start(RnetKeypadId keypadId)
        {
            return Start(SynchronizationContext.Current, keypadId, CancellationToken.None);
        }

        /// <summary>
        /// Starts the RNET bus.
        /// </summary>
        /// <param name="keypadId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Start(SynchronizationContext synchronizationContext, RnetKeypadId keypadId, CancellationToken cancellationToken)
        {
            Contract.Requires<ArgumentNullException>(synchronizationContext != null);
            Contract.Requires<ArgumentNullException>(keypadId != RnetKeypadId.Null);
            Contract.Requires<ArgumentNullException>(cancellationToken != null);

            using (await lck.LockAsync(cancellationToken))
            {
                if (state == RnetBusState.Started)
                    throw new RnetException("Bus is already started or starting.");

                // our intentions are to be started
                state = RnetBusState.Started;

                // begin starting
                SetState(RnetBusState.Starting);
                SynchronizationContext = synchronizationContext;
                Controllers.Clear();

                // generate minimal required model and insert new local device
                var c = Controllers[RnetControllerId.Root];
                var z = c.Zones[RnetZoneId.Zone1];
                var d = new RnetLocalDevice(z, keypadId);
                z.Devices[keypadId] = LocalDevice = d;

                // start client running
                await Client.Start();

                // activate the path to the local device
                d.Activate();

                // initiate device scan
                await Scan();

                SetState(RnetBusState.Started);
            }
        }

        /// <summary>
        /// Stops the RNET bus.
        /// </summary>
        /// <returns></returns>
        public Task Stop()
        {
            return Stop(CancellationToken.None);
        }

        /// <summary>
        /// Stops the RNET bus.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Stop(CancellationToken cancellationToken)
        {
            using (await lck.LockAsync())
            {
                if (state != RnetBusState.Started)
                    throw new RnetException("Bus is already stopped or stopping.");

                // our intentions are to be stopped
                state = RnetBusState.Stopped;

                SetState(RnetBusState.Stopping);
                await Client.Stop();
                SetState(RnetBusState.Stopped);
            }
        }

        /// <summary>
        /// Invoked when the connection state is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void Connection_StateChanged(object sender, RnetConnectionStateEventArgs args)
        {
            UpdateState();
        }

        /// <summary>
        /// Invoked when the client state is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void Client_StateChanged(object sender, RnetClientStateEventArgs args)
        {
            UpdateState();
        }

        /// <summary>
        /// Invoked when a message arrives.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void Client_MessageReceived(object sender, RnetMessageEventArgs args)
        {
            SynchronizationContext.Post(i => OnClientMessageReceived(args), null);
        }

        /// <summary>
        /// Invoked when a message is sent.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void Client_MessageSent(object sender, RnetMessageEventArgs args)
        {
            SynchronizationContext.Post(i => OnMessageSent(args), null);
        }

        /// <summary>
        /// Invoked when an error is emitted by the client.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void Client_UnhandledException(object sender, RnetExceptionEventArgs args)
        {
            SynchronizationContext.Post(i => OnUnhandledException(args), null);
        }

        /// <summary>
        /// Invoked when a message arrives.
        /// </summary>
        /// <param name="args"></param>
        async void OnClientMessageReceived(RnetMessageEventArgs args)
        {
            Contract.Requires<ArgumentNullException>(args != null);

            // client is stopped, ignore any trailing events
            if (State != RnetBusState.Started)
                return;

            // unknown
            var message = args.Message;
            if (message == null)
                return;

            // find device from which the message originated
            var device = GetOrCreateDevice(message.SourceDeviceId) as RnetRemoteDevice;
            if (device != null)
            {
                // device has been seen
                device.Activate();

                // destined to us
                if (message.TargetDeviceId == LocalDevice.DeviceId)
                    await device.SentMessage(message);

                // destined to all devices
                if (message.TargetDeviceId == RnetDeviceId.AllDevices)
                    await device.SentMessage(message);

                // destined to all devices on our zone
                if (message.TargetDeviceId.KeypadId == RnetKeypadId.AllZone &&
                    message.TargetDeviceId.ZoneId == LocalDevice.DeviceId.ZoneId)
                    await device.SentMessage(message);
            }

            OnMessageReceived(args);
        }

        /// <summary>
        /// Gets a handle to the device given the specified device id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        RnetDevice this[RnetDeviceId id]
        {
            get { return GetOrCreateDevice(id); }
        }

        /// <summary>
        /// Gets or creates a handle to the device given the specified device id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        RnetDevice GetOrCreateDevice(RnetDeviceId id)
        {
            // get controller
            var c = Controllers[id.ControllerId];
            if (c == null)
                return null;

            // we are a controller
            if (id.ZoneId == 0 &&
                id.KeypadId == RnetKeypadId.Controller)
                return c;

            // get zone
            var z = c.Zones[id.ZoneId];
            if (z == null)
                return null;

            // get device
            var d = z.Devices[id.KeypadId];
            if (d == null)
                return null;

            return d;
        }

        /// <summary>
        /// Scans for known devices.
        /// </summary>
        /// <returns></returns>
        public async Task Scan()
        {
            await Controllers.Scan();
        }

        /// <summary>
        /// Raised when a message is sent.
        /// </summary>
        public event EventHandler<RnetMessageEventArgs> MessageSent;

        /// <summary>
        /// Raises the MessageSent event.
        /// </summary>
        /// <param name="args"></param>
        void OnMessageSent(RnetMessageEventArgs args)
        {
            Contract.Requires<ArgumentNullException>(args != null);

            if (MessageSent != null)
                MessageSent(this, args);
        }

        /// <summary>
        /// Raised when a new message is received.
        /// </summary>
        public event EventHandler<RnetMessageEventArgs> MessageReceived;

        /// <summary>
        /// Raises the MessageReceived event.
        /// </summary>
        /// <param name="args"></param>
        void OnMessageReceived(RnetMessageEventArgs args)
        {
            Contract.Requires<ArgumentNullException>(args != null);

            if (MessageReceived != null)
                MessageReceived(this, args);
        }

        /// <summary>
        /// Raised when the bus state changes.
        /// </summary>
        public event EventHandler<RnetBusStateEventArgs> StateChanged;

        /// <summary>
        /// Raises the StateChanged event.
        /// </summary>
        /// <param name="args"></param>
        void OnStateChanged(RnetBusStateEventArgs args)
        {
            Contract.Requires<ArgumentNullException>(args != null);

            if (StateChanged != null)
                StateChanged(this, args);
        }

        /// <summary>
        /// Raised when an error occurs.
        /// </summary>
        public event EventHandler<RnetExceptionEventArgs> UnhandledException;

        /// <summary>
        /// Raises the Error event.
        /// </summary>
        /// <param name="args"></param>
        void OnUnhandledException(RnetExceptionEventArgs args)
        {
            Contract.Requires<ArgumentNullException>(args != null);

            if (UnhandledException != null)
                UnhandledException(this, args);
        }

        /// <summary>
        /// Disposes of the instance if possible.
        /// </summary>
        public void Dispose()
        {
            try
            {
                Stop().Wait();
            }
            catch (Exception e)
            {
                Trace.Fail("Exception in Dispose", e.ToString());
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes the instance.
        /// </summary>
        ~RnetBus()
        {
            Dispose();
        }

    }

}
