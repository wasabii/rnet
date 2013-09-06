using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

namespace Rnet.Service
{

    public sealed class CompositionService : ICompositionService
    {

        CompositionContainer container;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="container"></param>
        internal CompositionService(CompositionContainer container)
        {
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
