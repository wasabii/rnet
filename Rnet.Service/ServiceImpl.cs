using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using Nancy.Bootstrappers.Mef.Composition.Hosting;
using Nito.AsyncEx;
using Rnet.Service.Host;

namespace Rnet.Service
{

    /// <summary>
    /// Implements the RNet service.
    /// </summary>
    class ServiceImpl : IDisposable
    {

        /// <summary>
        /// Describes a running instance.
        /// </summary>
        class Instance
        {

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="sync"></param>
            /// <param name="bus"></param>
            /// <param name="host"></param>
            public Instance(AsyncContextThread sync, RnetBus bus, RnetHost host)
            {
                Context = sync;
                Bus = bus;
                Host = host;
            }

            public AsyncContextThread Context { get; set; }

            public RnetBus Bus { get; set; }

            public RnetHost Host { get; set; }

        }

        readonly CompositionContainer container;
        readonly List<Instance> instances;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ServiceImpl()
        {
            this.container = new CompositionContainer(new ApplicationCatalog(), new NancyExportProvider());
            this.instances = new List<Instance>();
        }

        public void OnStart(string[] args)
        {
            var sect = RnetServiceConfigurationSection.GetDefaultSection();
            if (sect == null)
                throw new ConfigurationErrorsException("Rnet.Service configuration not found.");

            foreach (var conf in sect.Hosts)
            {
                // expose new bus on a new host
                var context = new AsyncContextThread();
                var bus = new RnetBus(conf.Bus);
                var host = new RnetHost(bus, conf.Uri, container);
                instances.Add(new Instance(context, bus, host));

                // schedule initialization
                context.Factory.Run(async () =>
                {
                    await bus.Start();
                    await host.StartAsync();
                }).Wait();
            }
        }

        public void OnStop()
        {
            foreach (var item in instances.ToArray())
            {
                var context = item.Context;
                var bus = item.Bus;
                var host = item.Host;

                // signal stop from within context
                item.Context.Factory.Run(async () =>
                {
                    await host.StopAsync();
                    await bus.Stop();
                });

                // wait for exit
                context.JoinAsync();
                instances.Remove(item);
            }
        }

        public void Dispose()
        {
            OnStop();
        }

    }

}
