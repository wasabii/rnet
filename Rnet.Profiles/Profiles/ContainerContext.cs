namespace Rnet.Drivers.Profiles
{

    /// <summary>
    /// Provides information related to the current position of a <see cref="RnetBusObject"/> in the object tree.
    /// </summary>
    public sealed class ContainerContext
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        internal ContainerContext(IBusObjectOwner owner, IBusObjectContainer container)
        {
            Set(owner, container);
        }

        /// <summary>
        /// Owner of the nested bus object.
        /// </summary>
        public IBusObjectOwner Owner { get; private set; }

        /// <summary>
        /// Container of the nested bus object.
        /// </summary>
        public IBusObjectContainer Container { get; private set; }

        /// <summary>
        /// Updates the values.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="container"></param>
        internal void Set(IBusObjectOwner owner, IBusObjectContainer container)
        {
            Owner = owner;
            Container = container;
        }

    }

}
