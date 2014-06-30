using System;
using System.Diagnostics.Contracts;
using System.Net;
using System.Threading.Tasks;

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
        /// <param name="root"></param>
        protected RequestProcessor(
            RootProcessor root)
        {
            Contract.Requires<ArgumentNullException>(root != null);

            Root = root;
        }

        /// <summary>
        /// Module of the current request.
        /// </summary>
        protected RootProcessor Root { get; private set; }

        /// <summary>
        /// Resolves a new target from the given target.
        /// </summary>
        /// <returns></returns>
        public abstract Task<object> Resolve(IContext context, T target, string[] path);

        /// <summary>
        /// Implements IRequestProcessor.Resolve.
        /// </summary>
        /// <returns></returns>
        Task<object> IRequestProcessor.Resolve(IContext context, object target, string[] path)
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
        public virtual Task<object> Get(IContext context, T target)
        {
            return Task.FromResult<object>(HttpStatusCode.MethodNotAllowed);
        }

        /// <summary>
        /// Implements IRequestProcessor.Get.
        /// </summary>
        /// <returns></returns>
        Task<object> IRequestProcessor.Get(IContext context, object target)
        {
            return Get(context, (T)target);
        }

        /// <summary>
        /// Implements a PUT request.
        /// </summary>
        /// <returns></returns>
        public virtual Task<object> Put(IContext context, T target)
        {
            return Task.FromResult<object>(HttpStatusCode.MethodNotAllowed);
        }

        /// <summary>
        /// Implements IRequestProcessor.Put.
        /// </summary>
        /// <returns></returns>
        Task<object> IRequestProcessor.Put(IContext context, object target)
        {
            return Put(context, (T)target);
        }

    }

}
