using System;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Nancy.Bootstrapper.Mef
{

    /// <summary>
    /// Catalog version that wraps another 
    /// </summary>
    public class NancyAssemblyCatalog : ComposablePartCatalog
    {

        Assembly assembly;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="assembly"></param>
        public NancyAssemblyCatalog(Assembly assembly)
        {
            Contract.Requires<NullReferenceException>(assembly != null);

            this.assembly = assembly;
        }

        public override System.Linq.IQueryable<ComposablePartDefinition> Parts
        {
            get
            {
                return base.Parts;
            }
        }

    }

}
