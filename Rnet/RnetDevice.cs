using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

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
            Messages = new BlockingCollection<RnetMessage>();
            DataItems = new RnetDataItemCollection(this);
            DataItems.CollectionChanged += DataItems_CollectionChanged;
            DataItemsTree = new RnetDataTreeRoot(this);
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
        /// Reference to the communications bus.
        /// </summary>
        public RnetBus Bus { get; protected set; }

        /// <summary>
        /// Gets the ID of the device on the RNET bus.
        /// </summary>
        public abstract RnetDeviceId Id { get; }

        /// <summary>
        /// Messages incoming to from this device.
        /// </summary>
        public AsyncCollection<RnetMessage> Messages { get; private set; }

        /// <summary>
        /// Gets the set of data items stored in this device.
        /// </summary>
        public RnetDataItemCollection DataItems { get; private set; }

        /// <summary>
        /// Gets the tree organized set of data items stored in this device.
        /// </summary>
        public RnetDataTreeRoot DataItemsTree { get; private set; }

        /// <summary>
        /// Invoked when the data items collection is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void DataItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    DataItemsTree.Add(args.NewItems.Cast<RnetDataItem>());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    DataItemsTree.Remove(args.OldItems.Cast<RnetDataItem>());
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Initiates a request for the data at the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal async Task<RnetDataItem> RequestDataItem(RnetPath path, CancellationToken cancellationToken)
        {
            var m = await RequestData(Id, path, cancellationToken);
            if (m.PacketCount > 1)
                throw new NotSupportedException("Multi-package messages not supported.");

            var d = new RnetDataItem(path);
            d.WriteBegin(m.PacketCount);
            d.Write(m.Data.ToArray(), m.PacketNumber);
            d.WriteEnd();
            return d;
        }

        /// <summary>
        /// Schedules the given conversation for execution.
        /// </summary>
        /// <param name="conversation"></param>
        internal async Task<T> Converse<T>(RnetMessage message, Func<RnetMessage, T> getReply, CancellationToken cancellationToken)
            where T : class
        {
            if (message == null)
                throw new ArgumentNullException("message");

            cancellationToken.ThrowIfCancellationRequested();

            // send initial conversation message
            Bus.Client.SendMessage(message);

            // wait for reply
            while (getReply != null)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // wait for new possible reply message
                var r = await Messages.TryTakeAsync(cancellationToken);
                if (!r.Success)
                    continue;

                // extract message
                var m = r.Item;
                if (m == null)
                    continue;

                // check message timestamp
                if (m.Timestamp < message.Timestamp)
                    continue;

                // is this message a valid reply?
                var reply = getReply(m);
                if (reply != null)
                    return reply;

                // retry initial packet
                if (message != null)
                    Bus.Client.SendMessage(message);
            }

            return null;
        }

        /// <summary>
        /// Initiates a request for data for the specified device and path.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="targetPath"></param>
        internal async Task<RnetSetDataMessage> RequestData(RnetDeviceId deviceId, RnetPath targetPath, CancellationToken cancellationToken)
        {
            // add a conversation that sends a request data message and expects a set data message in return
            return await Converse(new RnetRequestDataMessage(deviceId, Id,
                targetPath,
                null,
                RnetRequestMessageType.Data),
                i =>
                {
                    // expected set data message
                    var msg = i as RnetSetDataMessage;
                    if (msg != null &&
                        msg.SourceDeviceId == deviceId &&
                        msg.SourcePath == targetPath)
                        return msg;

                    return null;
                }, cancellationToken);
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
