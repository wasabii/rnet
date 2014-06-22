using System;
using System.Diagnostics.Contracts;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Rnet.Service.Host.Processors
{

    /// <summary>
    /// <see cref="IRequestProcessor"/> instance designed to handle a specific type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class RequestProcessor<T> :
        IRequestProcessor
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="module"></param>
        protected RequestProcessor(
            RootRequestProcessor module)
        {
            Contract.Requires<ArgumentNullException>(module != null);

            Module = module;
        }

        /// <summary>
        /// Module of the current request.
        /// </summary>
        protected RootRequestProcessor Module { get; private set; }

        /// <summary>
        /// Resolves a new target from the given target.
        /// </summary>
        /// <returns></returns>
        public abstract Task<object> Resolve(IOwinContext context, T target, string[] path);

        /// <summary>
        /// Implements IRequestProcessor.Resolve.
        /// </summary>
        /// <returns></returns>
        Task<object> IRequestProcessor.Resolve(IOwinContext context, object target, string[] path)
        {
            if (path.Length == 0)
                return Task.FromResult(target);
            else
                return Resolve(context, (T)target, path);
        }

        /// <summary>
        /// Implements a GET request.
        /// </summary>
        /// <returns></returns>
        public virtual Task<object> Get(IOwinContext context, T target)
        {
            return Task.FromResult<object>(HttpStatusCode.MethodNotAllowed);
        }

        /// <summary>
        /// Implements IRequestProcessor.Get.
        /// </summary>
        /// <returns></returns>
        Task<object> IRequestProcessor.Get(IOwinContext context, object target)
        {
            return Get(context, (T)target);
        }

        /// <summary>
        /// Implements a PUT request.
        /// </summary>
        /// <returns></returns>
        public virtual Task<object> Put(IOwinContext context, T target)
        {
            return Task.FromResult<object>(HttpStatusCode.MethodNotAllowed);
        }

        /// <summary>
        /// Implements IRequestProcessor.Put.
        /// </summary>
        /// <returns></returns>
        Task<object> IRequestProcessor.Put(IOwinContext context, object target)
        {
            return Put(context, (T)target);
        }

    }

}
