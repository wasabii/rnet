using System;
using System.Threading.Tasks;

namespace Rnet.Drivers.Profiles
{

    /// <summary>
    /// Provides functionality for devices that own other bus objects. This is usually implemented along with
    /// <see cref="IBusObjectContainer"/> to provide information to nested objects.
    /// </summary>
    public interface IBusObjectOwner
    {

        /// <summary>
        /// The <see cref="RnetDevice"/> which is the owner of other objects.
        /// </summary>
        RnetDevice Device { get; }

        /// <summary>
        /// Obtains the profile implementation of the requested type for the given nested <see cref="RnetBusObject"/>.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="profileType"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<object> GetProfile(RnetBusObject target, Type profileType, ContainerContext context);

    }

}
