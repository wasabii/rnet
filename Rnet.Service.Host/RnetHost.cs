using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.Contracts;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Nito.AsyncEx;
using Owin;
using Rnet.Service.Host.Win32;

namespace Rnet.Service.Host
{

    public class RnetHost :
        IDisposable
    {

        readonly AsyncLock async = new AsyncLock();
        readonly RnetBus bus;
        readonly string baseUri;
        readonly CompositionContainer container;
        IDisposable webApp;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="baseUri"></param>
        /// <param name="exports"></param>
        public RnetHost(RnetBus bus, string baseUri, CompositionContainer exports)
        {
            Contract.Requires<ArgumentNullException>(bus != null);
            Contract.Requires<ArgumentNullException>(baseUri != null);
            Contract.Requires<ArgumentNullException>(exports != null);
            Contract.Requires<ArgumentException>(baseUri.EndsWith("/"));

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
        /// <param name="exports"></param>
        public RnetHost(RnetBus bus, Uri baseUri, CompositionContainer exports)
            : this(bus, baseUri.ToString(), exports)
        {
            Contract.Requires<ArgumentNullException>(bus != null);
            Contract.Requires<ArgumentNullException>(baseUri != null);
            Contract.Requires<ArgumentNullException>(exports != null);
            Contract.Requires<ArgumentException>(baseUri.ToString().EndsWith("/"));
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

                // allocate URL listener
                //HttpApi.ReserveUrl(baseUri, WindowsIdentity.GetCurrent().User);

                // spawn web application
                webApp = WebApp.Start(new StartOptions(baseUri), _ =>
                {
                    _.Use(async (context, func) =>
                    {
                        await container.GetExportedValue<RootProcessor>().Invoke(new OwinContext(context));
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

        /// <summary>
        /// Finalizes the instance.
        /// </summary>
        ~RnetHost()
        {
            Dispose();
        }

    }

}
