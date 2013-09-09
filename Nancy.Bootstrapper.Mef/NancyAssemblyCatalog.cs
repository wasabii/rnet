using System;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Nancy.Bootstrapper.Mef
{

    /// <summary>
    /// Assembly catalog that provides export of Nancy implementations that are not decorated with standard
    /// MEF attributes. The <see cref="NancyReflectionContext"/> is used to virtualize MEF attributes.
    /// </summary>
    public class NancyAssemblyCatalog : NancyCatalog
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="assembly"></param>
        public NancyAssemblyCatalog(Assembly assembly)
            : base(new AssemblyCatalog(assembly, new NancyReflectionContext()))
        {
            Contract.Requires<NullReferenceException>(assembly != null);
        }

    }

}
