using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Nancy.Hosting.Self;
using Nito.AsyncEx;

namespace Rnet.Service.Host
{

    public class RnetHost : IDisposable
    {

        readonly AsyncLock async = new AsyncLock();
        readonly RnetBus bus;
        readonly Uri baseUri;
        readonly AggregateCatalog catalog;
        readonly CompositionContainer container;
        NancyHost nancy;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="baseUri"></param>
        /// <param name="container"></param>
        public RnetHost(RnetBus bus, Uri baseUri, CompositionContainer container)
        {
            Contract.Requires<ArgumentNullException>(bus != null);
            Contract.Requires<ArgumentNullException>(baseUri != null);
            Contract.Requires<ArgumentNullException>(container != null);
            Contract.Requires<ArgumentException>(baseUri.ToString().EndsWith("/"));

            this.bus = bus;
            this.baseUri = baseUri;
            this.container = container;

            // export initial values
            container.ComposeExportedValue<ICompositionService>(new CompositionService(container));
            container.ComposeExportedValue(this);
            container.ComposeExportedValue(bus);
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="baseUri"></param>
        public RnetHost(RnetBus bus, string baseUri, CompositionContainer container)
            : this(bus, new Uri(baseUri), container)
        {
            Contract.Requires<ArgumentNullException>(bus != null);
            Contract.Requires<ArgumentNullException>(baseUri != null);
            Contract.Requires<ArgumentNullException>(container != null);
            Contract.Requires<ArgumentException>(baseUri.EndsWith("/"));
        }

        /// <summary>
        /// Starts the host.
        /// </summary>
        public void Start()
        {
            StartAsync().Wait();
        }

        /// <summary>
        /// Starts the service, from within synchronization context.
        /// </summary>
        public async Task StartAsync()
        {
            using (await async.LockAsync())
            {
                await Task.Yield();

                // configure nancy
                nancy = new NancyHost(
                    new RnetNancyBootstrapper(container),
                    baseUri);
                nancy.Start();
            }
        }

        /// <summary>
        /// Stops the host.
        /// </summary>
        public void Stop()
        {
            StopAsync().Wait();
        }

        /// <summary>
        /// Stops the service, from within synchronization context.
        /// </summary>
        public async Task StopAsync()
        {
            using (await async.LockAsync())
            {
                await Task.Yield();

                if (nancy != null)
                {
                    nancy.Stop();
                    nancy.Dispose();
                    nancy = null;
                }
            }
        }

        /// <summary>
        /// Disposes of the instance.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        ///// <summary>
        ///// Implements IStatusCodeHandler.HandlesStatusCode.
        ///// </summary>
        ///// <param name="statusCode"></param>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //bool IStatusCodeHandler.HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        //{
        //    Console.WriteLine("{0} {1} : {2}", (int)statusCode, statusCode, context.Request.Url);

        //    // check whether the reported status code is an error
        //    return statusCode != HttpStatusCode.OK;
        //}

        ///// <summary>
        ///// Implements IStatusCodeHandler.Handle.
        ///// </summary>
        ///// <param name="statusCode"></param>
        ///// <param name="context"></param>
        //void IStatusCodeHandler.Handle(HttpStatusCode statusCode, NancyContext context)
        //{
        //    Console.WriteLine("{0} {1} : {2}", (int)statusCode, statusCode, context.Request.Url);
        //}

        /// <summary>
        /// Finalizes the instance.
        /// </summary>
        ~RnetHost()
        {
            Dispose();
        }

    }

}
