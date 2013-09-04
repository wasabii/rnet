using System;
using System.ComponentModel.Composition;
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
        /// <param name="target"></param>
        protected RequestProcessor(
            ObjectModule module)
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
        protected ObjectModule Module { get; private set; }

        /// <summary>
        /// Separated path of the target.
        /// </summary>
        protected string[] Path
        {
            get { return Module.Target.Path; }
        }

        /// <summary>
        /// Gets the object being targetted by the request.
        /// </summary>
        protected T Object
        {
            get { return (T)Module.Target.Object; }
        }

        /// <summary>
        /// Implements a GET request.
        /// </summary>
        /// <returns></returns>
        public abstract Task<object> Get();

        /// <summary>
        /// Implements IRequestProcessor.Get.
        /// </summary>
        /// <returns></returns>
        Task<object> IRequestProcessor.Get()
        {
            return Get();
        }

    }

}
