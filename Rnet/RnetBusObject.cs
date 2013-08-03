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
        /// Gets a string to describe the object.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets a set of extensions associated with this object.
        /// </summary>
        public RnetExtensionsCollection Extensions { get; private set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name ?? base.ToString();
        }

    }

}
