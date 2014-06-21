using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Microsoft.Owin.Hosting;

using Nito.AsyncEx;

using Owin;

namespace Rnet.Service.Host
{

    public class RnetHost :
        IDisposable
    {

        readonly AsyncLock async = new AsyncLock();
        readonly RnetBus bus;
        readonly Uri baseUri;
        readonly CompositionContainer container;
        IDisposable webApp;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="baseUri"></param>
        /// <param name="exports"></param>
        public RnetHost(RnetBus bus, Uri baseUri, CompositionContainer exports)
        {
            Contract.Requires<ArgumentNullException>(bus != null);
            Contract.Requires<ArgumentNullException>(baseUri != null);
            Contract.Requires<ArgumentNullException>(exports != null);
            Contract.Requires<ArgumentException>(baseUri.ToString().EndsWith("/"));

            this.bus = bus;
            this.baseUri = baseUri;
            this.container = exports;

            // export initial values
            exports.ComposeExportedValue<ICompositionService>(new CompositionService(exports));
            exports.ComposeExportedValue(this);
            exports.ComposeExportedValue(bus);
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="baseUri"></param>
        public RnetHost(RnetBus bus, string baseUri, CompositionContainer exports)
            : this(bus, new Uri(baseUri), exports)
        {
            Contract.Requires<ArgumentNullException>(bus != null);
            Contract.Requires<ArgumentNullException>(baseUri != null);
            Contract.Requires<ArgumentNullException>(exports != null);
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

                webApp = WebApp.Start(new StartOptions(baseUri.ToString()), _ =>
                {
                    _.Use(async (context, func) =>
                    {
                        await container.GetExportedValue<BusModule>().Invoke(context);
                        await func();
                    });
                });
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

                if (webApp != null)
                {
                    webApp.Dispose();
                    webApp = null;
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
        //bool IStatusCodeHandler.HandlesStatusCode(HttpStatusCode statusCode, IOwinContext context)
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
        //void IStatusCodeHandler.Handle(HttpStatusCode statusCode, IOwinContext context)
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
