using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.AttributedModel;
using System.Diagnostics.Contracts;
using Nancy.Hosting.Self;
using System.ComponentModel.Composition.Registration;

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
        Uri uri = new Uri("rnet.tcp://tokyo.cogito.cx:9999");
        ApplicationCatalog applicationCatalog;
        AggregateCatalog catalog;
        CompositionContainer container;
        RnetBus bus;
        NancyHost nancyHost;

        public void OnStart()
        {
            sync.Post(i => OnStartAsync(), null);
            sync.Start();
        }

        /// <summary>
        /// Starts the service, from within synchronization context.
        /// </summary>
        async void OnStartAsync()
        {
            Contract.Requires(container == null);
            Contract.Requires(bus == null);
            Contract.Requires(nancyHost == null);

            // configure the application container
            container = new CompositionContainer(
                catalog = new AggregateCatalog(applicationCatalog = new ApplicationCatalog()),
                CompositionOptions.DisableSilentRejection | CompositionOptions.IsThreadSafe | CompositionOptions.ExportCompositionService);



            // configure bus
            bus = new RnetBus(uri);
            await bus.Start();
            container.ComposeExportedValue<RnetBus>(bus);

            // configure nancy
            nancyHost = new NancyHost(new NancyBootstrapper(container), new Uri("http://localhost:12292/rnet/"));
            nancyHost.Start();
        }

        public void OnStop()
        {
            Contract.Assert(sync != null);

            sync.Post(i => OnStopAsync(), null);
            sync.Stop();
        }

        /// <summary>
        /// Stops the service, from within synchronization context.
        /// </summary>
        async void OnStopAsync()
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
