using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rnet
{

    /// <summary>
    /// Provides a durable view of the state of an RNET system.
    /// </summary>
    public class RnetBus : RnetModelObject, IDisposable
    {

        /// <summary>
        /// Initializes the static instance.
        /// </summary>
        static RnetBus()
        {
            RnetUriParser.RegisterParsers();
        }

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
        /// Gets the default cancellation token.
        /// </summary>
        /// <returns></returns>
        internal CancellationToken DefaultTimeoutToken
        {
            get { return new CancellationTokenSource(TimeSpan.FromSeconds(60)).Token; }
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
        /// Gets the current state of the client.
        /// </summary>
        public RnetClientState ClientState
        {
            get { return Client.State; }
        }

        /// <summary>
        /// Gets the current state of the connection.
        /// </summary>
        public RnetConnectionState ConnectionState
        {
            get { return Client.ConnectionState; }
        }

        /// <summary>
        /// Device node representing ourselves.
        /// </summary>
        public RnetBusDevice Device { get; private set; }

        /// <summary>
        /// RNET devices detected on the bus.
        /// </summary>
        public RnetControllerCollection Controllers { get; private set; }

        /// <summary>
        /// Starts the RNET bus.
        /// </summary>
        /// <returns></returns>
        public Task StartAsync()
        {
            return StartAsync(RnetKeypadId.External);
        }

        /// <summary>
        /// Starts the RNET bus.
        /// </summary>
        /// <param name="keypadId"></param>
        /// <returns></returns>
        public Task StartAsync(RnetKeypadId keypadId)
        {
            return StartAsync(keypadId, CancellationToken.None);
        }

        /// <summary>
        /// Starts the RNET bus.
        /// </summary>
        /// <param name="keypadId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(RnetKeypadId keypadId, CancellationToken cancellationToken)
        {
            // default to caller's
            // TODO implement internal fall back
            SynchronizationContext = SynchronizationContext.Current;

            // required for the bus
            if (SynchronizationContext == null)
                throw new RnetException("A synchronization context must be provided.");

            // empty out any existing controllers
            Controllers.Clear();

            // generate minimal required model and insert new bus device
            var c = Controllers[RnetControllerId.Root];
            var z = c.Zones[RnetZoneId.Zone1];
            var d = new RnetBusDevice(z, keypadId);
            z.Devices[keypadId] = Device = d;

            // start new client
            Client = new RnetClient(Uri);
            Client.StateChanged += Client_StateChanged;
            Client.ConnectionStateChanged += Client_ConnectionStateChanged;
            Client.MessageReceived += Client_MessageReceived;
            Client.MessageSent += Client_MessageSent;
            Client.Error += Client_Error;
            await Client.StartAsync();

            // activate the path to the bus device
            d.Activate();

            // initiate device scan
            await Scan();
        }

        /// <summary>
        /// Stops the RNET bus.
        /// </summary>
        /// <returns></returns>
        public Task StopAsync()
        {
            return StopAsync(CancellationToken.None);
        }

        /// <summary>
        /// Stops the RNET bus.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (Client == null)
                throw new ObjectDisposedException("RnetBus");

            // wait for theclient to stop
            await Client.StopAsync();

            // unset so users won't get confused by stale data
            Controllers.Clear();
            Device = null;
        }

        /// <summary>
        /// Invoked when the client's state changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void Client_StateChanged(object sender, RnetClientStateEventArgs args)
        {
            SynchronizationContext.Post(i => OnClientStateChanged(args), null);
        }

        /// <summary>
        /// Invoked when the connection's state changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void Client_ConnectionStateChanged(object sender, RnetConnectionStateEventArgs args)
        {
            SynchronizationContext.Post(i => OnConnectionStateChanged(args), null);
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
        void Client_Error(object sender, RnetClientErrorEventArgs args)
        {
            SynchronizationContext.Post(i => OnError(args), null);
        }

        /// <summary>
        /// Invoked when a message arrives.
        /// </summary>
        /// <param name="args"></param>
        async void OnClientMessageReceived(RnetMessageEventArgs args)
        {
            // client is stopped, ignore any trailing events
            if (Client.State != RnetClientState.Started)
                return;

            // skip messages not destined to us or everybody
            var message = args.Message;
            if (message == null)
                return;

            // generate or obtain device and allow it to handle message
            var device = GetOrCreateDevice(message.SourceDeviceId);
            if (device != null)
            {
                // destined to us
                if (message.TargetDeviceId == Device.DeviceId)
                    await device.ReceiveMessage(message);

                // destined to all devices
                if (message.TargetDeviceId == RnetDeviceId.AllDevices)
                    await device.ReceiveMessage(message);

                // destined to all devices on our zone
                if (message.TargetDeviceId.KeypadId == RnetKeypadId.AllZone &&
                    message.TargetDeviceId.ZoneId == Device.DeviceId.ZoneId)
                    await device.ReceiveMessage(message);
            }

            // raise event on synchronization context
            SynchronizationContext.Post(i => OnMessageReceived(args), null);
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
            if (MessageReceived != null)
                MessageReceived(this, args);
        }

        /// <summary>
        /// Raised when an error occurs.
        /// </summary>
        public event EventHandler<RnetClientErrorEventArgs> Error;

        /// <summary>
        /// Raises the Error event.
        /// </summary>
        /// <param name="args"></param>
        void OnError(RnetClientErrorEventArgs args)
        {
            if (Error != null)
                Error(this, args);
        }

        /// <summary>
        /// Raised when the state changes.
        /// </summary>
        public event EventHandler<RnetClientStateEventArgs> ClientStateChanged;

        /// <summary>
        /// Raises the ClientStateChanged event.
        /// </summary>
        /// <param name="args"></param>
        void OnClientStateChanged(RnetClientStateEventArgs args)
        {
            if (ClientStateChanged != null)
                ClientStateChanged(this, args);
            RaisePropertyChanged("ClientState");
        }

        /// <summary>
        /// Raised when the connection state changes.
        /// </summary>
        public event EventHandler<RnetConnectionStateEventArgs> ConnectionStateChanged;

        /// <summary>
        /// Raises the ConnectionStateChanged event.
        /// </summary>
        /// <param name="args"></param>
        void OnConnectionStateChanged(RnetConnectionStateEventArgs args)
        {
            if (ConnectionStateChanged != null)
                ConnectionStateChanged(this, args);
            RaisePropertyChanged("ConnectionState");
        }

        /// <summary>
        /// Disposes of the instance if possible.
        /// </summary>
        public void Dispose()
        {
            if (Client != null)
            {
                try
                {
                    StopAsync().Wait();
                    Client.StateChanged -= Client_StateChanged;
                    Client.ConnectionStateChanged -= Client_ConnectionStateChanged;
                    Client.MessageReceived -= Client_MessageReceived;
                    Client.MessageSent -= Client_MessageSent;
                    Client.Error -= Client_Error;
                    Client.Dispose();
                    Client = null;
                }
                catch (Exception)
                {

                }
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
