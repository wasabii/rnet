namespace Rnet
{
    /// <summary>
    /// Base class for objects made available on the RNET bus.
    /// </summary>
    public abstract class RnetObject
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        internal protected RnetObject()
        {
            Context = new RnetContext();
        }

        /// <summary>
        /// Gets a set of items associated with this object.
        /// </summary>
        public RnetContext Context { get; private set; }

    }

}
