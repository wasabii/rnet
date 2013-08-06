using System;

namespace Rnet
{

    /// <summary>
    /// Base class for objects made available on the RNET bus.
    /// </summary>
    public abstract class RnetBusObject
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        internal protected RnetBusObject(RnetBus bus)
        {
            Bus = bus;
            Context = new RnetContext();
            Timestamp = DateTime.MinValue;
        }

        /// <summary>
        /// Reference to the communications bus.
        /// </summary>
        public RnetBus Bus { get; protected set; }

        /// <summary>
        /// Gets a set of items associated with this object.
        /// </summary>
        public RnetContext Context { get; private set; }

        /// <summary>
        /// Time of the last contact with this object.
        /// </summary>
        public DateTime Timestamp { get; private set; }

        /// <summary>
        /// Returns <c>true</c> if this bus object is still considered active.
        /// </summary>
        public virtual bool IsActive
        {
            get { return Timestamp >= DateTime.UtcNow.AddDays(-1); }
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
