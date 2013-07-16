using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rnet
{

    public class RnetBus : RnetModelObject, IDisposable
    {

        /// <summary>
        /// Gets the default cancellation token.
        /// </summary>
        /// <returns></returns>
        internal static CancellationToken CreateDefaultCancellationToken()
        {
            return CancellationToken.None;
        }

        /// <summary>
        /// Signals to the bus to stop all threads.
        /// </summary>
        CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="connection"></param>
        public RnetBus(RnetConnection connection, RnetDeviceId id, SynchronizationContext synchronizationContext)
        {
            // we are our own bus
            SynchronizationContext = synchronizationContext;

            if (connection == null)
                throw new ArgumentNullException("client");

            // hook ourselves up to the client
            Client = new RnetClient(connection);
            Client.StateChanged += Client_StateChanged;
            Client.ConnectionStateChanged += Client_ConnectionStateChanged;
            Client.MessageReceived += Client_MessageReceived;
            Client.MessageSent += Client_MessageSent;
            Client.Error += Client_Error;

            // initialize set of known devices, including ourselves
            ClientDevice = new RnetClientDevice(this, id);
            Devices = new RnetDeviceCollection(this);
            Devices.Add(ClientDevice);
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="client"></param>
        public RnetBus(RnetConnection connection)
            : this(connection, RnetDeviceId.External, SynchronizationContext.Current)
        {

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
        public RnetClientDevice ClientDevice { get; private set; }

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
                throw new ObjectDisposedException("RnetBus");

            Client.Start();
        }

        /// <summary>
        /// Stops the RNET bus.
        /// </summary>
        public void Stop()
        {
            if (Client == null)
                throw new ObjectDisposedException("RnetBus");

            Client.Stop();

            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource = null;
            }
        }

        /// <summary>
        /// Invoked when the client's state changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Client_StateChanged(object sender, RnetClientStateEventArgs args)
        {
            SynchronizationContext.Post(i => OnClientStateChanged(args), null);
        }

        /// <summary>
        /// Invoked when the connection's state changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Client_ConnectionStateChanged(object sender, RnetConnectionStateEventArgs args)
        {
            SynchronizationContext.Post(i => OnConnectionStateChanged(args), null);
        }

        /// <summary>
        /// Invoked when a message arrives.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        async void Client_MessageReceived(object sender, RnetMessageEventArgs args)
        {
            // client is stopped, ignore any trailing events
            if (Client.State != RnetClientState.Started)
                return;

            // skip messages not destined to us
            var message = args.Message;
            if (message.TargetDeviceId != ClientDevice.Id &&
                message.TargetDeviceId != RnetDeviceId.AllDevices)
                return;

            // add message to device queue
            var device = Devices[message.SourceDeviceId];
            if (device != null)
                await device.ReceiveMessage(message);

            SynchronizationContext.Post(i => OnMessageReceived(args), null);
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
        /// Sends a RequestData message to the specified device id with the specified paths.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="targetPath"></param>
        /// <param name="sourcePath"></param>
        internal void SendRequestDataMessage(RnetDeviceId deviceId, RnetPath targetPath, RnetPath sourcePath)
        {
            Client.SendMessage(new RnetRequestDataMessage(deviceId, ClientDevice.Id, targetPath, sourcePath, RnetRequestMessageType.Data));
        }

        /// <summary>
        /// Requests the device with the given <see cref="RnetDeviceId"/>.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        internal async Task<RnetDevice> RequestDevice(RnetDeviceId deviceId, CancellationToken cancellationToken)
        {
            var device = Devices[deviceId];
            if (device != null)
                return device;

            // establish device before requesting data
            if (deviceId.ZoneId == RnetZoneId.Zone1 &&
                deviceId.KeypadId == RnetKeypadId.Controller)
                Devices.Add(device = new RnetController(this, deviceId.ControllerId));

            // prime device model if possible; else remove
            if (device != null)
                if (await device.Data.GetAsync(new RnetPath(0, 0), cancellationToken) == null)
                    Devices.Remove(device);

            // set the device to visible if we've successfully detected it
            device = Devices[deviceId];
            if (device != null)
            {
                device.ModelName = Encoding.ASCII.GetString((await device.Data.GetAsync(new RnetPath(0, 0), cancellationToken)).Buffer).Trim();
                device.Visible = true;
            }

            return device;
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
                Stop();
                Client.StateChanged -= Client_StateChanged;
                Client.ConnectionStateChanged -= Client_ConnectionStateChanged;
                Client.MessageReceived -= Client_MessageReceived;
                Client.MessageSent -= Client_MessageSent;
                Client.Error -= Client_Error;
                Client.Dispose();
                Client = null;
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
