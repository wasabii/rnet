namespace Rnet.Drivers
{

    /// <summary>
    /// Provides a profile implementation and the metadata associated with it.
    /// </summary>
    public sealed class Profile
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="metadata"></param>
        /// <param name="instance"></param>
        internal Profile(RnetBusObject target, ProfileMetadata metadata, object instance)
        {
            Target = target;
            Metadata = metadata;
            Instance = instance;
        }

        /// <summary>
        /// Gets the bus object which is targetted by the profile.
        /// </summary>
        public RnetBusObject Target { get; private set; }

        /// <summary>
        /// Gets the metadata that describes the profile.
        /// </summary>
        public ProfileMetadata Metadata { get; private set; }

        /// <summary>
        /// Gets the instance which provides the implementation of the profile. This object implements the service
        /// contract referenced in the metadata.
        /// </summary>
        public object Instance { get; private set; }

    }

}
