using Rnet.Profiles.Metadata;

namespace Rnet.Service.Objects
{

    /// <summary>
    /// Holds a handle to a command.
    /// </summary>
    interface IProfileCommandHandle : IProfileHandle
    {

        /// <summary>
        /// Property to which we have a handle.
        /// </summary>
        CommandDescriptor Command { get; }

    }

}
