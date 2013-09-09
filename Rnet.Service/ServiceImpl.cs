using System;
using System.Collections.Generic;
using System.Configuration;

using Nito.AsyncEx;

using Rnet.Service.Host;

namespace Rnet.Service
{

    class ServiceImpl : IDisposable
    {

        class Execution
        {

            public Execution(AsyncContextThread sync, RnetBus bus, RnetHost host)
            {
                Context = sync;
                Bus = bus;
                Host = host;
            }

            public AsyncContextThread Context { get; set; }

            public RnetBus Bus { get; set; }

            public RnetHost Host { get; set; }

        }

        List<Execution> running;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ServiceImpl()
        {
            running = new List<Execution>();
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
                var host = new RnetHost(bus, conf.Uri);
                running.Add(new Execution(context, bus, host));

                // schedule initialization
                context.Factory.Run(async () =>
                {
                   // await bus.Start();
                    await host.StartAsync();
                }).Wait();
            }
        }

        public void OnStop()
        {
            foreach (var item in running.ToArray())
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
                running.Remove(item);
            }
        }

        public void Dispose()
        {
            OnStop();
        }

    }

}
