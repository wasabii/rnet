using System;
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
        /// Gets the default cancellation token.
        /// </summary>
        /// <returns></returns>
        internal static CancellationToken GetDefaultCancellationToken()
        {
            return CancellationToken.None;
        }

        /// <summary>
        /// Gets the cancellation token that drives timeouts.
        /// </summary>
        /// <returns></returns>
        internal static CancellationToken GetTimeoutCancellationToken()
        {
            return new CancellationTokenSource(TimeSpan.FromSeconds(2.5)).Token;
        }

        /// <summary>
        /// My device ID.
        /// </summary>
        RnetDeviceId id;

        /// <summary>
        /// Signals to the bus to stop all threads.
        /// </summary>
        CancellationTokenSource cancellationTokenSource;

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
            var msg = args.Message;
            if (msg.TargetDeviceId != Id &&
                msg.TargetDeviceId != RnetDeviceId.AllDevices)
                return;

            // add message to device queue
            var device = Devices[msg.SourceDeviceId];
            if (device != null)
                await device.Messages.AddAsync(msg);

            // handshake to acknowledge message
            var dmsg = msg as RnetSetDataMessage;
            if (dmsg != null)
                Client.SendMessage(new RnetHandshakeMessage(msg.SourceDeviceId, Id,
                    RnetHandshakeType.Data),
                    RnetMessagePriority.High);
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
                if (await device.DataItems.GetAsync(new RnetPath(0, 0), cancellationToken) == null)
                    Devices.Remove(device);

            // return whatever is left
            return Devices[deviceId];
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
