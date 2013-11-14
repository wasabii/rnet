using System;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.Contracts;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Mef;
using Nancy.Bootstrappers.Mef.Composition.Hosting;
using Nancy.ViewEngines;

namespace Rnet.Service.Host
{

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    public class RnetNancyBootstrapper : NancyBootstrapper
    {

        readonly CompositionContainer container;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="container"></param>
        public RnetNancyBootstrapper(CompositionContainer container)
        {
            Contract.Requires<ArgumentNullException>(container != null);

            this.container = container;
        }

        protected override CompositionContainer GetApplicationContainer()
        {
            return container;
        }

        protected override void ConfigureApplicationContainer(CompositionContainer existingContainer)
        {
            base.ConfigureApplicationContainer(existingContainer);

            ResourceViewLocationProvider.RootNamespaces.Add(
                typeof(RnetNancyBootstrapper).Assembly,
                "Rnet.Service.Host.Views");
        }

        protected override NancyInternalConfiguration InternalConfiguration
        {
            get
            {
                return NancyInternalConfiguration.WithOverrides(x =>
                {
                    //x.ViewLocationProvider = typeof(ResourceViewLocationProvider);
                });
            }
        }

        protected override void AddCatalog(CompositionContainer container, System.ComponentModel.Composition.Primitives.ComposablePartCatalog catalog)
        {
            base.AddCatalog(container, catalog);
        }

        //protected override void ApplicationStartup(CompositionContainer container, IPipelines pipelines)
        //{
        //    base.ApplicationStartup(container, pipelines);
        //}

    }

}
