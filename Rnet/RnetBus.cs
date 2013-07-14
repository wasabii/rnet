using System;
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

        object sync = new object();
        RnetDeviceId id;
        Timer timer;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="client"></param>
        public RnetBus(RnetClient client, RnetDeviceId id)
            : base(null)
        {
            // we are our own bus
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
            Devices.SubscriberAdded += Devices_SubscriberAdded;

            // periodically requests a refresh of all data
            timer = new Timer(timer_Run, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(60));
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="client"></param>
        public RnetBus(RnetClient client)
            : this(client, RnetDeviceId.External)
        {

        }

        public override RnetDeviceId Id
        {
            get { return id; }
        }

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
            }
        }

        /// <summary>
        /// Processes an incoming <see cref="RnetSetDataMessage"/>.
        /// </summary>
        /// <param name="msg"></param>
        void ProcessMessage(RnetSetDataMessage msg)
        {
            // non-external keypad IDs require a handshake to acknowledge message
            if (Id.KeypadId != RnetKeypadId.External)
                Client.SendMessage(new RnetHandshakeMessage(msg.SourceDeviceId, Id, RnetHandshakeType.Data));

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
                    device.DataItems.WriteBegin(msg.SourcePath);

                // write data of packet
                device.DataItems.Write(msg.SourcePath, msg.Data.ToArray());

                // end write on last packet
                if (msg.PacketNumber == msg.PacketCount - 1)
                    device.DataItems.WriteEnd(msg.SourcePath);
            }
        }

        /// <summary>
        /// Invoked periodicially.
        /// </summary>
        /// <param name="state"></param>
        void timer_Run(object state)
        {
            lock (sync)
            {
                foreach (var device in Devices)
                {
                    // remove expired items
                    foreach (var item in device.DataItems.ToList())
                        if ((DateTime.UtcNow - item.Timestamp) > TimeSpan.FromMinutes(15))
                            if (item.Buffer != null)
                                device.DataItems.Remove(item.Path);

                    // remove devices with no items
                    if (device != this)
                        if (!device.DataItems.Any())
                            Devices.Remove(device);
                }

                // only continue if we're currently connected
                if (Client.ConnectionState != RnetConnectionState.Open)
                    return;

                //// request all zone info for each zone
                //for (byte i = 0; i < 6; i++)
                //    Client.SendMessage(new RnetRequestDataMessage(RnetDeviceId.RootController, Id,
                //        new RnetPath(2, 0, i, 7),
                //        null,
                //        RnetRequestMessageType.Data));
            }
        }

        /// <summary>
        /// Invoked when a subscriber is added for a device id.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void Devices_SubscriberAdded(object sender, ValueEventArgs<RnetDeviceId> args)
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
