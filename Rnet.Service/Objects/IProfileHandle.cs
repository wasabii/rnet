using Rnet.Drivers;

namespace Rnet.Service.Objects
{

    /// <summary>
    /// Holds a handle to a profile.
    /// </summary>
    interface IProfileHandle
    {

        /// <summary>
        /// Profile to which we have a handle.
        /// </summary>
        ProfileHandle Profile { get; }

    }

}
