using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Nancy;

using Rnet.Service.Objects;

namespace Rnet.Service.Processors
{

    /// <summary>
    /// <see cref="IRequestProcessor"/> instance designed to handle a specific type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class RequestProcessor<T> : IRequestProcessor
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="module"></param>
        protected RequestProcessor(
            BusModule module)
        {
            Contract.Requires<ArgumentNullException>(module != null);

            Module = module;
        }

        /// <summary>
        /// Context of the current request.
        /// </summary>
        protected NancyContext Context
        {
            get { return Module.Context; }
        }

        /// <summary>
        /// Module of the current request.
        /// </summary>
        protected BusModule Module { get; private set; }

        /// <summary>
        /// Resolves a new target from the given target.
        /// </summary>
        /// <returns></returns>
        public abstract Task<object> Resolve(T target, string[] path);

        /// <summary>
        /// Implements IRequestProcessor.Resolve.
        /// </summary>
        /// <returns></returns>
        Task<object> IRequestProcessor.Resolve(object target, string[] path)
        {
            if (path.Length == 0)
                return Task.FromResult(target);
            else
                return Resolve((T)target, path);
        }

        /// <summary>
        /// Implements a GET request.
        /// </summary>
        /// <returns></returns>
        public virtual Task<object> Get(T target)
        {
            return Task.FromResult<object>(HttpStatusCode.MethodNotAllowed);
        }

        /// <summary>
        /// Implements IRequestProcessor.Get.
        /// </summary>
        /// <returns></returns>
        Task<object> IRequestProcessor.Get(object target)
        {
            return Get((T)target);
        }

        /// <summary>
        /// Implements a PUT request.
        /// </summary>
        /// <returns></returns>
        public virtual Task<object> Put(T target)
        {
            return Task.FromResult<object>(HttpStatusCode.MethodNotAllowed);
        }

        /// <summary>
        /// Implements IRequestProcessor.Put.
        /// </summary>
        /// <returns></returns>
        Task<object> IRequestProcessor.Put(object target)
        {
            return Put((T)target);
        }

    }

}
