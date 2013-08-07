namespace Rnet.Profiles
{

    /// <summary>
    /// Provides information related to the current position of a <see cref="RnetBusObject"/> in the object tree.
    /// </summary>
    class ContainerContext : IContainerContext
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        internal ContainerContext(RnetBusObject owner, RnetBusObject container)
        {
            Set(owner, container);
        }

        /// <summary>
        /// Owner of the nested bus object.
        /// </summary>
        public RnetBusObject Owner { get; private set; }

        /// <summary>
        /// Container of the nested bus object.
        /// </summary>
        public RnetBusObject Container { get; private set; }

        /// <summary>
        /// Updates the values.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="container"></param>
        internal void Set(RnetBusObject owner, RnetBusObject container)
        {
            Owner = owner;
            Container = container;
        }

    }

}
