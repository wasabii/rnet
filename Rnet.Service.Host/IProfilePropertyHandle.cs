using Rnet.Profiles.Metadata;

namespace Rnet.Service.Host.Models
{

    /// <summary>
    /// Holds a handle to a property.
    /// </summary>
    interface IProfilePropertyHandle : IProfileHandle
    {

        /// <summary>
        /// Property to which we have a handle.
        /// </summary>
        PropertyDescriptor Property { get; }

    }

}
