namespace Rnet.Profiles
{

    /// <summary>
    /// Describes the relationship of a nested bus object to its owner and its current container.
    /// </summary>
    public interface IContainerContext
    {

        /// <summary>
        /// Owner of the nested bus object.
        /// </summary>
        RnetBusObject Owner { get; }

        /// <summary>
        /// Container of the nested bus object.
        /// </summary>
        RnetBusObject Container { get; }

    }

}
