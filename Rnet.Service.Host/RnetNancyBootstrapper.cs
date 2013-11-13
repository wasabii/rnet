using System;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.Contracts;

using Nancy.Bootstrappers.Mef;

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

    }

}
