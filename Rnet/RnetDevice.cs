using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Rnet
{

    /// <summary>
    /// Base class of RNET devices.
    /// </summary>
    public abstract class RnetDevice : RnetModelObject
    {

        List<AsyncCollectionSubscriber<RnetDataItem>> dataItemSubscribers =
            new List<AsyncCollectionSubscriber<RnetDataItem>>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        RnetDevice()
        {
            DataItems = new RnetDataItemCollection();
            DataItems.CollectionChanged += DataItems_CollectionChanged;
            DataItems.RequestData += DataItems_RequestData;
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
        /// Invoked when a subscriber for a path is added.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void DataItems_RequestData(object sender, ValueEventArgs<RnetPath> args)
        {
            Bus.RequestData(Id, args.Value);
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

        /// <summary>
        /// Gets the tree organized set of data items stored in this device.
        /// </summary>
        public RnetDataTreeRoot DataItemsTree { get; private set; }

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
