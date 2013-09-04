using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Nancy.Hosting.Self;

namespace Rnet.Service
{

    class Host : IDisposable
    {

        /// <summary>
        /// Initializes the static instance.
        /// </summary>
        static Host()
        {
            Rnet.Drivers.Russound.DriverPackage.Register();
        }

        SingleThreadSynchronizationContext sync = new SingleThreadSynchronizationContext();
        Uri uri = new Uri("rnet.tcp://70.123.112.92:9999");
        ApplicationCatalog applicationCatalog;
        AggregateCatalog catalog;
        CompositionContainer container;
        RnetBus bus;
        NancyHost nancyHost;

        public Host()
        {
            sync.UnhandledException += sync_UnhandledException;
        }

        /// <summary>
        /// Invoked when there is a unhandled exception.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void sync_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            if (Debugger.IsAttached)
                Debugger.Break();
        }

        public void OnStart()
        {
            sync.Post(async i => await OnStartAsync(), null);
            sync.Start();
        }

        /// <summary>
        /// Starts the service, from within synchronization context.
        /// </summary>
        async Task OnStartAsync()
        {
            // configure the application container
            container = new CompositionContainer(
                catalog = new AggregateCatalog(applicationCatalog = new ApplicationCatalog()),
                CompositionOptions.DisableSilentRejection | CompositionOptions.IsThreadSafe | CompositionOptions.ExportCompositionService);
            container.ComposeExportedValue<ICompositionService>(new CompositionService(container));

            // configure bus
            bus = new RnetBus(uri);
            container.ComposeExportedValue<RnetBus>(bus);

            // configure nancy
            nancyHost = new NancyHost(
                new NancyBootstrapper(container),
                new Uri("http://localhost:12292/rnet/"));
            nancyHost.Start();

            // start the bus
            await bus.Start();
        }

        /// <summary>
        /// Stops the service.
        /// </summary>
        public void OnStop()
        {
            Contract.Assert(sync != null);

            sync.Post(async i => await OnStopAsync(), null);
            sync.Stop();
        }

        /// <summary>
        /// Stops the service, from within synchronization context.
        /// </summary>
        async Task OnStopAsync()
        {
            if (container != null)
            {
                container.Dispose();
                container = null;
            }

            if (nancyHost != null)
            {
                nancyHost.Stop();
                nancyHost.Dispose();
                nancyHost = null;
            }

            if (bus != null)
            {
                await bus.Stop();
                bus = null;
            }
        }

        public void Dispose()
        {
            OnStop();
        }

    }

}
