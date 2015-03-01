using System;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.Contracts;

namespace Rnet.Service.Host
{

    public sealed class CompositionService : ICompositionService
    {

        readonly CompositionContainer container;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="container"></param>
        internal CompositionService(CompositionContainer container)
        {
            Contract.Requires<ArgumentNullException>(container != null);

            this.container = container;
        }

        public CompositionContainer Container
        {
            get { return container; }
        }

        public void SatisfyImportsOnce(ComposablePart part)
        {
            container.SatisfyImportsOnce(part);
        }

    }

}
