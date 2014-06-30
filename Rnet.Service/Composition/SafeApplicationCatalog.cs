using System;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

namespace Rnet.Service
{

    /// <summary>
    /// <see cref="ComposablePartCatalog"/> that does not fail when parts cannot be loaded.
    /// </summary>
    class SafeApplicationCatalog :
        ComposablePartCatalog
    {

        readonly ComposablePartCatalog catalog;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public SafeApplicationCatalog()
        {
            this.catalog = new SafeDirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory);
        }

        public override IQueryable<ComposablePartDefinition> Parts
        {
            get { return catalog.Parts; }
        }

    }

}
