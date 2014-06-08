using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;

namespace Rnet.Service
{

    public class SafeDirectoryCatalog :
        ComposablePartCatalog
    {

        readonly AggregateCatalog catalog;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public SafeDirectoryCatalog(string directory)
        {
            this.catalog = new AggregateCatalog(GetAssemblyCatalogs(directory));
        }

        IEnumerable<ComposablePartCatalog> GetAssemblyCatalogs(string directory)
        {
            return Directory.EnumerateFiles(directory, "*.dll")
                .Select(i => new SafeAssemblyCatalog(i));
        }

        public override IQueryable<ComposablePartDefinition> Parts
        {
            get { return catalog.Parts; }
        }

    }

}
