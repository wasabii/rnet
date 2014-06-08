using System;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

namespace Rnet.Service
{

    class SafeApplicationCatalog :
        ComposablePartCatalog
    {

        readonly ComposablePartCatalog catalog;

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
