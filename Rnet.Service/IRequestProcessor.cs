using System.Threading.Tasks;

namespace Rnet.Service.Processors
{

    /// <summary>
    /// Represents a class capable of handling a request against an object with a certain set of qualifications.
    /// </summary>
    public interface IRequestProcessor
    {

        /// <summary>
        /// Returns the result from the given request.
        /// </summary>
        /// <returns></returns>
        Task<object> Get();

    }

}
