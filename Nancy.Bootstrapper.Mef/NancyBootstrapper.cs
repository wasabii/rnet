using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.ReflectionModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Nancy.Diagnostics;

namespace Nancy.Bootstrapper.Mef
{

    /// <summary>
    /// Serves as a bootstrapper for Nancy when using the Managed Extensibility Framework.
    /// </summary>
    [InheritedExport(typeof(INancyBootstrapper))]
    [InheritedExport(typeof(INancyModuleCatalog))]
    public class NancyBootstrapper : NancyBootstrapperWithRequestContainerBase<CompositionContainer>
    {

        AggregateCatalog catalog;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public NancyBootstrapper()
            : base()
        {

        }

        #region Application Container

        /// <summary>
        /// Gets the default catalog configured on the container. This is always an <see cref="AggregateCatalog"/> to
        /// make later expansion easier.
        /// </summary>
        public AggregateCatalog AplicationContainerCatalog
        {
            get { return catalog; }
        }

        /// <summary>
        /// Create a default, unconfigured, container
        /// </summary>
        /// <returns>Container instance</returns>
        protected override CompositionContainer GetApplicationContainer()
        {
            return new CompositionContainer(
                catalog = GetApplicationCatalog(),
                CompositionOptions.IsThreadSafe | CompositionOptions.ExportCompositionService);
        }

        /// <summary>
        /// Creates the catalog to associate with the default application container.
        /// </summary>
        /// <returns></returns>
        protected virtual AggregateCatalog GetApplicationCatalog()
        {
            return new AggregateCatalog();
        }

        /// <summary>
        /// Provides a place to configure the newly created <see cref="CompositionContainer"/>. By default this method
        /// registers a <see cref="NancyAssemblyCatalog"/> associated with the Nancy assembly. This prevents multiple
        /// catalogs from being added later during individual type registration.
        /// </summary>
        /// <param name="existingContainer"></param>
        protected override void ConfigureApplicationContainer(CompositionContainer existingContainer)
        {
            AplicationContainerCatalog.Catalogs.Add(new NancyAssemblyCatalog(typeof(NancyEngine).Assembly));
        }

        /// <summary>
        /// Returns <c>true</c> if the type given by <see cref="implementationType"/> is already available as an export
        /// of <see cref="contractType"/> in the container.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="contractType"></param>
        /// <param name="implementationType"></param>
        /// <returns></returns>
        bool CatalogHasPart(CompositionContainer container, Type contractType, Type implementationType)
        {
            return container.Catalog
                .Where(i => i.Exports(contractType))
                .Where(i => ReflectionModelServices.GetPartType(i).Value.UnderlyingSystemType == implementationType.UnderlyingSystemType)
                .Any();
        }

        /// <summary>
        /// Bind the bootstrapper's implemented types into the container. This is necessary so a user can pass in a
        /// populated container but not have to take the responsibility of registering things like <see
        /// cref="INancyModuleCatalog"/> manually.
        /// </summary>
        /// <param name="applicationContainer">Application container to register into</param>
        protected override sealed void RegisterBootstrapperTypes(CompositionContainer applicationContainer)
        {
            applicationContainer.ComposeParts(this);
        }

        /// <summary>
        /// Bind the default implementations of internally used types into the container as singletons.
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="typeRegistrations">Type registrations to register</param>
        protected override sealed void RegisterTypes(CompositionContainer container, IEnumerable<TypeRegistration> typeRegistrations)
        {
            Contract.Requires<ArgumentNullException>(container != null);
            Contract.Requires<ArgumentNullException>(typeRegistrations != null);

            //
            var types = typeRegistrations
                .Where(i => !CatalogHasPart(container, i.RegistrationType, i.ImplementationType))
                .SelectMany(i => new[] { i.ImplementationType, i.RegistrationType })
                .Select(i => i.UnderlyingSystemType)
                .Distinct()
                .OrderBy(i => i.FullName);
            if (types.Any())
                ((AggregateCatalog)container.Catalog).Catalogs.Add(new NancyTypeCatalog(types));
        }

        /// <summary>
        /// Bind the various collections into the container as singletons to later be resolved by IEnumerable{Type}
        /// constructor dependencies.
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="collectionTypeRegistrations">Collection type registrations to register</param>
        protected override sealed void RegisterCollectionTypes(CompositionContainer container, IEnumerable<CollectionTypeRegistration> collectionTypeRegistrations)
        {
            Contract.Requires<ArgumentNullException>(container != null);
            Contract.Requires<ArgumentNullException>(collectionTypeRegistrations != null);

            RegisterTypes(container, collectionTypeRegistrations
                .SelectMany(i => i.ImplementationTypes.Select(j => new TypeRegistration(i.RegistrationType, j))));
        }

        /// <summary>
        /// Bind the given instances into the container.
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="instanceRegistrations">Instance registration types</param>
        protected override void RegisterInstances(CompositionContainer container, IEnumerable<InstanceRegistration> instanceRegistrations)
        {
            Contract.Requires<ArgumentNullException>(container != null);
            Contract.Requires<ArgumentNullException>(instanceRegistrations != null);

            RegisterTypes(container, instanceRegistrations.Select(i => new TypeRegistration(i.RegistrationType, i.Implementation.GetType())));
            foreach (var r in instanceRegistrations)
                container.ComposeExportedValue(r.RegistrationType, r.Implementation);
        }

        #endregion

        #region Request Container

        /// <summary>
        /// Creates a per-request container. The default MEF implemenetation simply creates a new <see cref="Composition"/>
        /// container that uses the first one as a <see cref="ExportProvider"/> as well as an initial part catalog.
        /// </summary>
        /// <returns></returns>
        protected override CompositionContainer CreateRequestContainer()
        {
            Contract.Requires<ArgumentNullException>(ApplicationContainer != null);

            return new CompositionContainer(
                new AggregateCatalog(ApplicationContainer.Catalog),
                CompositionOptions.IsThreadSafe | CompositionOptions.ExportCompositionService,
                ApplicationContainer);
        }

        /// <summary>
        /// Registers per-request modules in the per-request container.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="moduleRegistrationTypes"></param>
        protected override void RegisterRequestContainerModules(CompositionContainer container, IEnumerable<ModuleRegistration> moduleRegistrationTypes)
        {
            Contract.Requires<ArgumentNullException>(container != null);
            Contract.Requires<ArgumentNullException>(moduleRegistrationTypes != null);
        }

        #endregion

        /// <summary>
        /// Gets the diagnostics for initialization.
        /// </summary>
        protected override IDiagnostics GetDiagnostics()
        {
            Contract.Requires<ArgumentNullException>(ApplicationContainer != null);

            return ApplicationContainer.GetExportedValue<IDiagnostics>();
        }
            
        /// <summary>
        /// Gets all registered application startup tasks.
        /// </summary>
        protected override IEnumerable<IApplicationStartup> GetApplicationStartupTasks()
        {
            Contract.Requires<ArgumentNullException>(ApplicationContainer != null);

            return ApplicationContainer.GetExportedValues<IApplicationStartup>();
        }

        /// <summary>
        /// Gets all registered application registration tasks.
        /// </summary>
        protected override IEnumerable<IApplicationRegistrations> GetApplicationRegistrationTasks()
        {
            Contract.Requires<ArgumentNullException>(ApplicationContainer != null);

            return ApplicationContainer.GetExportedValues<IApplicationRegistrations>();
        }

        /// <summary>
        /// Gets the engine implementation from the container.
        /// </summary>
        protected override sealed INancyEngine GetEngineInternal()
        {
            Contract.Requires<ArgumentNullException>(ApplicationContainer != null);

            return ApplicationContainer.GetExportedValueOrDefault<INancyEngine>();
        }

        /// <summary>
        /// Retrieve all module instances from the container.
        /// </summary>
        protected override sealed IEnumerable<INancyModule> GetAllModules(CompositionContainer container)
        {
            Contract.Requires<ArgumentNullException>(container != null);

            return container.GetExportedValues<INancyModule>();
        }

        /// <summary>
        /// Retreive a specific module instance from the container.
        /// </summary>
        /// <param name="container">Container to use</param>
        /// <param name="moduleType">Type of the module</param>
        protected override INancyModule GetModule(CompositionContainer container, Type moduleType)
        {
            Contract.Requires<ArgumentNullException>(container != null);
            Contract.Requires<ArgumentNullException>(moduleType != null);

            return container.GetExports<INancyModule>()
                .Select(i => i.Value)
                .FirstOrDefault(i => i.GetType() == moduleType);
        }

    }

}
