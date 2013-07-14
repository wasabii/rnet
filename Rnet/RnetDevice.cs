namespace Rnet
{

    /// <summary>
    /// Base class of RNET devices.
    /// </summary>
    public abstract class RnetDevice : RnetModelObject
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        RnetDevice()
        {
            DataItems = new RnetDataItemCollection();
            DataItems.SubscriberAdded += DataItems_SubscriberAdded;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected RnetDevice(RnetBus bus)
            : this()
        {
            Bus = bus;
        }

        /// <summary>
        /// Invoked when a subscriber for a path is added.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void DataItems_SubscriberAdded(object sender, ValueEventArgs<RnetPath> args)
        {
            // send request message for data at path
            Bus.Client.SendMessage(new RnetRequestDataMessage(Id, Bus.Id,
                args.Value,
                null,
                RnetRequestMessageType.Data));
        }

        /// <summary>
        /// Gets the ID of the device on the RNET bus.
        /// </summary>
        public abstract RnetDeviceId Id { get; }

        /// <summary>
        /// Reference to the communications bus.
        /// </summary>
        public RnetBus Bus { get; protected set; }

        /// <summary>
        /// Gets the set of data items stored in this device.
        /// </summary>
        public RnetDataItemCollection DataItems { get; private set; }

    }

}
