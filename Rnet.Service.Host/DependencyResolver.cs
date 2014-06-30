using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Dependencies;

namespace Rnet.Service.Host
{

    class DependencyResolver :
        IDependencyResolver,
        IDependencyScope
    {

        public CompositionContainer container;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="container"></param>
        public DependencyResolver(CompositionContainer container)
        {
            Contract.Requires<ArgumentNullException>(container != null);

            this.container = container;
        }

        public IDependencyScope BeginScope()
        {
            return this;
        }

        public object GetService(Type serviceType)
        {
            return container.GetExports(
                new ContractBasedImportDefinition(
                    AttributedModelServices.GetContractName(serviceType),
                    AttributedModelServices.GetTypeIdentity(serviceType),
                    null,
                    ImportCardinality.ZeroOrMore,
                    false,
                    false,
                    CreationPolicy.Any))
                .Select(i => i.Value)
                .FirstOrDefault();
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return container.GetExports(
                new ContractBasedImportDefinition(
                    AttributedModelServices.GetContractName(serviceType),
                    AttributedModelServices.GetTypeIdentity(serviceType),
                    null,
                    ImportCardinality.ZeroOrMore,
                    false,
                    false,
                    CreationPolicy.Any))
                .Select(i => i.Value);
        }

        public void Dispose()
        {

        }

    }

}
