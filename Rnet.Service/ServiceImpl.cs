using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Diagnostics.Contracts;
using Nito.AsyncEx;
using Rnet.Service.Host;

namespace Rnet.Service
{

    /// <summary>
    /// Implements the RNet service.
    /// </summary>
    class ServiceImpl :
        IDisposable
    {

        static readonly SafeApplicationCatalog catalog = new SafeApplicationCatalog();

        /// <summary>
        /// Describes a running instance.
        /// </summary>
        class Instance
        {

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="container"></param>
            /// <param name="sync"></param>
            /// <param name="bus"></param>
            /// <param name="host"></param>
            public Instance(CompositionContainer container, AsyncContextThread sync, RnetBus bus, RnetHost host)
            {
                Contract.Requires<ArgumentNullException>(container != null);
                Contract.Requires<ArgumentNullException>(sync != null);
                Contract.Requires<ArgumentNullException>(bus != null);
                Contract.Requires<ArgumentNullException>(host != null);

                Container = container;
                Context = sync;
                Bus = bus;
                Host = host;
            }

            public CompositionContainer Container { get; set; }

            public AsyncContextThread Context { get; set; }

            public RnetBus Bus { get; set; }

            public RnetHost Host { get; set; }

        }

        readonly List<Instance> instances;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ServiceImpl()
        {
            this.instances = new List<Instance>();
        }

        /// <summary>
        /// Invoke this method upon start.
        /// </summary>
        /// <param name="args"></param>
        public void OnStart(string[] args)
        {
            Contract.Requires<ArgumentNullException>(args != null);

            var sect = RnetServiceConfigurationSection.GetDefaultSection();
            if (sect == null)
                throw new ConfigurationErrorsException("Rnet.Service configuration not found.");

            // spawn each host
            foreach (var conf in sect.Hosts)
            {
                // expose new bus on a new host with new container
                var container = new CompositionContainer(catalog);
                var context = new AsyncContextThread();
                var bus = new RnetBus(conf.Bus);
                var host = new RnetHost(bus, conf.Uri, container);
                instances.Add(new Instance(container, context, bus, host));

                // schedule initialization
                context.Factory.Run(async () =>
                {
                    await bus.Start();
                    await host.StartAsync();
                }).Wait();
            }
        }

        /// <summary>
        /// Invoke this method upon stop.
        /// </summary>
        public void OnStop()
        {
            foreach (var item in instances.ToArray())
            {
                var container = item.Container;
                var context = item.Context;
                var bus = item.Bus;
                var host = item.Host;

                // signal stop from within context
                item.Context.Factory.Run(async () =>
                {
                    await host.StopAsync();
                    await bus.Stop();
                    
                    // dispose container
                    container.Dispose();
                });

                // wait for exit
                context.JoinAsync();
                instances.Remove(item);
            }
        }

        /// <summary>
        /// Disposes of the instance.
        /// </summary>
        public void Dispose()
        {
            OnStop();
        }

    }

}
