using System;
using System.Diagnostics.Contracts;

namespace Rnet
{

    /// <summary>
    /// Base class for objects which are underneath the bus.
    /// </summary>
    public abstract class RnetBusObject : RnetObject
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        internal protected RnetBusObject(RnetBus bus)
            : base()
        {
            Contract.Requires<ArgumentNullException>(bus != null);

            Bus = bus;
            Timestamp = DateTime.MinValue;
        }

        /// <summary>
        /// Reference to the communications bus.
        /// </summary>
        public RnetBus Bus { get; protected set; }

        /// <summary>
        /// Time of the last contact with this object.
        /// </summary>
        public DateTime Timestamp { get; private set; }

        /// <summary>
        /// Returns <c>true</c> if this bus object is still considered active.
        /// </summary>
        public virtual bool IsActive
        {
            get { return Timestamp >= DateTime.UtcNow.AddDays(-365); }
        }

        /// <summary>
        /// Marks the object as active, raising whatever events are required.
        /// </summary>
        public virtual void Activate()
        {
            Timestamp = DateTime.UtcNow;
        }

    }

}
