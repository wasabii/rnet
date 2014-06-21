using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;

namespace Rnet.Service
{

    /// <summary>
    /// <see cref="ComposablePartCatalog"/> that does not fail when parts cannot be loaded.
    /// </summary>
    class SafeAssemblyCatalog :
        ComposablePartCatalog
    {

        readonly TypeCatalog catalog;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="assembly"></param>
        public SafeAssemblyCatalog(string assembly)
        {
            try
            {
                this.catalog = new TypeCatalog(GetTypes(assembly));
            }
            catch (TypeLoadException)
            {
                this.catalog = new TypeCatalog();
            }
        }

        /// <summary>
        /// Loads the <see cref="Type"/>s available in the specified assembly, or returns no types if the assembly
        /// cannot be loaded.
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        IEnumerable<Type> GetTypes(string assembly)
        {
            var types = new List<Type>();

            try
            {
                var asm = Assembly.LoadFrom(assembly);
                if (asm == null)
                    return types;

                foreach (var type in asm.GetTypes())
                {
                    try
                    {
                        type.GetMembers();
                        types.Add(type);
                    }
                    catch (TypeLoadException)
                    {

                    }
                }
            }
            catch (TypeLoadException)
            {

            }

            return types;
        }

        public override IQueryable<ComposablePartDefinition> Parts
        {
            get { return catalog.Parts; }
        }

    }

}
