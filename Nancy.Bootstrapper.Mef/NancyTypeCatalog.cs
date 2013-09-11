using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.Contracts;

namespace Nancy.Bootstrapper.Mef
{

    /// <summary>
    /// Type catalog that provides export of Nancy implementations that are not decorated with standard
    /// MEF attributes. The <see cref="NancyReflectionContext"/> is used to virtualize MEF attributes.
    /// </summary>
    public class NancyTypeCatalog : NancyCatalog
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="type"></param>
        public NancyTypeCatalog(params Type[] types)
            : this((IEnumerable<Type>)types)
        {
            Contract.Requires<NullReferenceException>(types != null);
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="type"></param>
        public NancyTypeCatalog(IEnumerable<Type> types)
            : base(new TypeCatalog(types, new NancyReflectionContext()))
        {
            Contract.Requires<NullReferenceException>(types != null);
        }

    }

}
