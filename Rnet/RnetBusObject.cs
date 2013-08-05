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
        }

        /// <summary>
        /// Gets a set of items associated with this object.
        /// </summary>
        public RnetContext Context { get; private set; }

    }

}
