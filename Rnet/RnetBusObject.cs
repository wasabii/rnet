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
            Extensions = new RnetExtensionsCollection();
        }

        /// <summary>
        /// Gets a set of extensions associated with this object.
        /// </summary>
        public RnetExtensionsCollection Extensions { get; private set; }

    }

}
