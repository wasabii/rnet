using System;
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
        /// Signals to the bus to stop all threads.
        /// </summary>
        CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="id"></param>
        /// <param name="synchronizationContext"></param>
        public RnetBus(RnetConnection connection, RnetKeypadId id, SynchronizationContext synchronizationContext)
        {
            if (connection == null)
                throw new ArgumentNullException("client");
            if (synchronizationContext == null)
                throw new ArgumentNullException("synchronizationContext");

            // we are our own bus
            SynchronizationContext = synchronizationContext;
            KeypadId = KeypadId;

            // hook ourselves up to the client
            Client = new RnetClient(connection);
            Client.StateChanged += Client_StateChanged;
            Client.ConnectionStateChanged += Client_ConnectionStateChanged;
            Client.MessageReceived += Client_MessageReceived;
            Client.MessageSent += Client_MessageSent;
            Client.Error += Client_Error;

            Controllers = new RnetControllerCollection(this);
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="connection"></param>
        public RnetBus(RnetConnection connection)
            : this(connection, RnetKeypadId.External, SynchronizationContext.Current)
        {

        }

        /// <summary>
        /// Gets the default cancellation token.
        /// </summary>
        /// <returns></returns>
        internal CancellationToken DefaultTimeoutToken
        {
            get { return new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token; }
        }

        /// <summary>
        /// Used to report events to the user.
        /// </summary>
        public SynchronizationContext SynchronizationContext { get; private set; }

        /// <summary>
        /// Keypad ID to use for bus client.
        /// </summary>
        public RnetKeypadId KeypadId { get; private set; }

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
        public RnetBusDevice BusDevice { get; private set; }

        /// <summary>
        /// RNET devices detected on the bus.
        /// </summary>
        public RnetControllerCollection Controllers { get; private set; }

        /// <summary>
        /// Starts the RNET bus.
        /// </summary>
        /// <returns></returns>
        public Task<RnetBusDevice> StartAsync()
        {
            return StartAsync(DefaultTimeoutToken);
        }

        /// <summary>
        /// Starts the RNET bus.
        /// </summary>
        public async Task<RnetBusDevice> StartAsync(CancellationToken cancellationToken)
        {
            if (Client == null)
                throw new ObjectDisposedException("RnetBus");

            // start client
            Client.Start();

            // attempt to acquire required node until cancelled
            while (BusDevice == null && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // initialize root controller, zone, and then our bus device
                    var controller = await Controllers.GetAsync(RnetControllerId.Root);
                    var zone = await controller.Zones.GetAsync(0);
                    var device = new RnetBusDevice(zone, KeypadId);

                    // set ourselves as the bus device
                    BusDevice = device;
                }
                catch (OperationCanceledException)
                {
                    // ignore
                }
            }

            return BusDevice;
        }

        /// <summary>
        /// Stops the RNET bus.
        /// </summary>
        /// <returns></returns>
        public Task StopAsync()
        {
            return StopAsync(DefaultTimeoutToken);
        }

        /// <summary>
        /// Stops the RNET bus.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (Client == null)
                throw new ObjectDisposedException("RnetBus");

            Client.Stop();

            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource = null;
            }

            return Task.FromResult(true);
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
        void Client_MessageReceived(object sender, RnetMessageEventArgs args)
        {
            SynchronizationContext.Post(i => OnClientMessageReceived(args), null);
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
            var device = await FindOrCreateAsync(message.SourceDeviceId);
            if (device != null)
                if (BusDevice == null ||
                    message.TargetDeviceId == RnetDeviceId.AllDevices ||
                    message.TargetDeviceId == BusDevice.DeviceId)
                    await device.ReceiveMessage(message);

            // raise event on synchronization context
            SynchronizationContext.Post(i => OnMessageReceived(args), null);
        }

        /// <summary>
        /// Finds an existing known device by the given device ID, or inserts it.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        async Task<RnetDevice> FindOrCreateAsync(RnetDeviceId deviceId)
        {
            if (BusDevice != null &&
                BusDevice.DeviceId == deviceId)
                return BusDevice;

            // find or create controller
            var controller = await Controllers.FindAsync(deviceId.ControllerId);
            if (controller == null)
                await Controllers.AddAsync(controller = new RnetController(this, new RnetControllerId(deviceId.ControllerId)));

            // find or create zone
            var zone = await controller.Zones.FindAsync(deviceId.ZoneId);
            if (zone == null)
                await controller.Zones.AddAsync(zone = new RnetZone(controller, deviceId.ZoneId));

            // find or create device
            var device = await zone.Devices.FindAsync(deviceId.KeypadId);
            if (device == null)
                await zone.Devices.AddAsync(device = new RnetZoneDevice(zone, deviceId.KeypadId));

            return device;
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
            Client.SendMessage(new RnetRequestDataMessage(
                deviceId,
                BusDevice != null ? BusDevice.DeviceId : RnetDeviceId.External,
                targetPath,
                sourcePath,
                RnetRequestMessageType.Data));
        }

        /// <summary>
        /// Requests the device with the given <see cref="RnetDeviceId"/>.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        internal async Task<RnetDevice> RequestAsync(RnetDeviceId deviceId, CancellationToken cancellationToken)
        {
            // request device information record
            SendRequestDataMessage(deviceId, new RnetPath(0, 0), RnetPath.Empty);

            // wait for controller
            var controller = await Controllers.WaitAsync(deviceId.ControllerId, cancellationToken);
            if (controller == null)
                return null;

            // we are the controller
            if (deviceId.KeypadId == RnetKeypadId.Controller)
                return controller;

            // wait for zone
            var zone = await controller.Zones.WaitAsync(deviceId.ZoneId, cancellationToken);
            if (zone == null)
                return null;

            // wait for device
            var device = await zone.Devices.WaitAsync(deviceId.KeypadId, cancellationToken);
            if (device == null)
                return null;

            return device;
        }

        /// <summary>
        /// Gets the device with the given ID if it's already known.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public Task<RnetDevice> FindAsync(RnetDeviceId deviceId)
        {
            return FindAsync(deviceId, DefaultTimeoutToken);
        }

        /// <summary>
        /// Gets the device with the given ID if it's already known.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<RnetDevice> FindAsync(RnetDeviceId deviceId, CancellationToken cancellationToken)
        {
            if (BusDevice != null &&
                BusDevice.DeviceId == deviceId)
                return BusDevice;

            // find the controller
            var controller = await Controllers.FindAsync(deviceId.ControllerId, cancellationToken);
            if (controller == null)
                return null;

            // we are the controller
            if (deviceId.KeypadId == RnetKeypadId.Controller)
                return controller;

            // find the zone
            var zone = await controller.Zones.FindAsync(deviceId.ZoneId, cancellationToken);
            if (zone == null)
                return null;

            // find the device
            var device = await zone.Devices.FindAsync(deviceId.KeypadId, cancellationToken);
            if (device == null)
                return null;

            return device;
        }

        /// <summary>
        /// Gets the device for the given device ID.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public async Task<RnetDevice> GetAsync(RnetDeviceId deviceId)
        {
            try
            {
                return await GetAsync(deviceId, DefaultTimeoutToken);
            }
            catch (OperationCanceledException)
            {
                // ignore
            }

            return null;
        }

        /// <summary>
        /// Gets the device for the given device ID using a generator function.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public async Task<RnetDevice> GetAsync(RnetDeviceId deviceId, CancellationToken cancellationToken)
        {
            if (BusDevice != null &&
                BusDevice.DeviceId == deviceId)
                return BusDevice;

            // obtain controller
            var controller = await Controllers.GetAsync(deviceId.ControllerId, cancellationToken);
            if (controller == null)
                return null;

            // we are looking for a controller
            if (deviceId.KeypadId == RnetKeypadId.Controller)
                return controller;

            // obtain zone
            var zone = await controller.Zones.GetAsync(deviceId.ZoneId, cancellationToken);
            if (zone == null)
                return null;

            // obtain device
            var device = await zone.Devices.GetAsync(deviceId.KeypadId, cancellationToken);
            if (device == null)
                return null;

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
                StopAsync().Wait();
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
