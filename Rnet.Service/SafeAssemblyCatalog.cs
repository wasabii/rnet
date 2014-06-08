using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;

namespace Rnet.Service
{

    class SafeAssemblyCatalog :
        ComposablePartCatalog
    {

        readonly TypeCatalog catalog;

        public SafeAssemblyCatalog(string assembly)
        {
            try
            {
                this.catalog = new TypeCatalog(GetTypes(assembly));
            }
            catch (ReflectionTypeLoadException e)
            {
                this.catalog = new TypeCatalog();
            }
        }

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
                    catch (TypeLoadException e)
                    {

                    }
                }
            }
            catch (TypeLoadException e)
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
