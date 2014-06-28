using System.Net;
using System.Threading.Tasks;

using Microsoft.Owin;

namespace Rnet.Service.Host.Processors
{

    [ResponseProcessor(typeof(HttpStatusCode))]
    public class HttpStatusCodeResponseProcessor :
        IResponseProcessor
    {

        public Task<bool> Handle(IContext context, object target)
        {
            context.Response.StatusCode = (int)(HttpStatusCode)target;
            return Task.FromResult(true);
        }

    }

}
