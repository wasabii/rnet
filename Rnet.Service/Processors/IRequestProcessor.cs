using System.Threading.Tasks;

using Nancy;

namespace Rnet.Service.Processors
{

    /// <summary>
    /// Represents a class capable of handling a request against an object with a certain set of qualifications.
    /// </summary>
    public interface IRequestProcessor
    {

        /// <summary>
        /// Returns <c>true</c> if the processor can handle the given request.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="method"></param>
        /// <param name="uri"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        Task<bool> CanProcess(NancyContext context, string method, string[] uri, object target);

        /// <summary>
        /// Returns the result from the given request.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="method"></param>
        /// <param name="uri"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        Task<object> Process(NancyContext context, string method, string[] uri, object target);

    }

}
