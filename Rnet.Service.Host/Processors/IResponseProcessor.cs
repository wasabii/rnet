using System.Threading.Tasks;

using Microsoft.Owin;

namespace Rnet.Service.Host.Processors
{

    /// <summary>
    /// Describes a class capable of handling an object and generating a response.
    /// </summary>
    public interface IResponseProcessor
    {

        /// <summary>
        /// Handles the response for the given target object.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        Task<bool> Handle(IContext context, object target);

    }

}
