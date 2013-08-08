using System.ComponentModel;
using System.ServiceModel;

namespace Rnet.Profiles
{

    /// <summary>
    /// Basic information exposed by any RNET object.
    /// </summary>
    [ServiceContract(Name = "object")]
    public interface IObject
    {

        /// <summary>
        /// Simple display name of the object.
        /// </summary>
        string DisplayName { get; }

    }

}
