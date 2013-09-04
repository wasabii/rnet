using System.Threading.Tasks;

using Nancy;

namespace Rnet.Service.Processors
{

    public abstract class GenericRequestProcessor<T> : IRequestProcessor
    {

        public abstract Task<bool> CanProcess(NancyContext context, string method, string[] uri, T target);

        public abstract Task<object> Process(NancyContext context, string method, string[] uri, T target);

        async Task<bool> IRequestProcessor.CanProcess(NancyContext context, string method, string[] uri, object target)
        {
            return target is T && await CanProcess(context, method, uri, (T)target);
        }

        Task<object> IRequestProcessor.Process(NancyContext context, string method, string[] uri, object target)
        {
            return Process(context, method, uri, (T)target);
        }

    }

}
