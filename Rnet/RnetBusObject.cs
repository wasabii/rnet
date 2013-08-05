using System;

namespace Rnet
{

    /// <summary>
    /// Base class for objects discovered on the RNET bus.
    /// </summary>
    public abstract class RnetBusObject : RnetModelObject
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected RnetBusObject()
        {
            Context = new RnetContext();
            Timestamp = DateTime.MinValue;
        }

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
        internal bool IsActive
        {
            get { return Timestamp >= DateTime.UtcNow.AddDays(-1); }
        }

        /// <summary>
        /// Marks the object as active, raising whatever events are required.
        /// </summary>
        internal virtual void Touch()
        {
            Timestamp = DateTime.UtcNow;
        }

    }

}
