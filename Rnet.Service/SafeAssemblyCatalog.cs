using System;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;

namespace Rnet.Service
{

    class SafeAssemblyCatalog :
        AssemblyCatalog
    {

        IQueryable<ComposablePartDefinition> parts;

        public SafeAssemblyCatalog(string assembly)
            : base(assembly)
        {
            try
            {
                this.parts = base.Parts.ToList().AsQueryable();
            }
            catch (ReflectionTypeLoadException e)
            {
                this.parts = Enumerable.Empty<ComposablePartDefinition>().AsQueryable();
                Console.WriteLine(assembly);
                Console.WriteLine(e);
            }
        }

        public override IQueryable<ComposablePartDefinition> Parts
        {
            get { return parts; }
        }

    }

}
