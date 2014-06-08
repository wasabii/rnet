using System.Threading.Tasks;

using Microsoft.Owin;

namespace Rnet.Service.Host.Processors
{

    /// <summary>
    /// Represents a class capable of handling a request against an object with a certain set of qualifications.
    /// </summary>
    public interface IRequestProcessor
    {

        /// <summary>
        /// Resolves a new object from an existing target. Return value can be another target, or any other type of
        /// request result.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        Task<object> Resolve(IOwinContext context, object target, string[] path);

        /// <summary>
        /// Handles and returns the result of a GET request against the specified target.
        /// </summary>
        /// <returns></returns>
        Task<object> Get(IOwinContext context, object target);

        /// <summary>
        /// Handles and returns the result of a PUT request against the specified target.
        /// </summary>
        /// <returns></returns>
        Task<object> Put(IOwinContext context, object target);

    }

}
