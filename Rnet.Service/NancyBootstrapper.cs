using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Registration;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

using Nancy;
using Nancy.Bootstrapper;
using Nancy.Diagnostics;

namespace Rnet.Service
{

    public class NancyBootstrapper : NancyBootstrapperWithRequestContainerBase<CompositionContainer>
    {

        static readonly HashSet<Type> interfaces = new HashSet<Type>(
            typeof(INancyEngine).Assembly.GetTypes()
                .Where(i => i.IsInterface)
                .Distinct());


        CompositionContainer parent;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public NancyBootstrapper(CompositionContainer parent)
            : base()
        {
            Contract.Requires(parent != null);
            this.parent = parent;
        }

        /// <summary>
        /// Create a default, unconfigured, container
        /// </summary>
        /// <returns>Container instance</returns>
        protected override CompositionContainer GetApplicationContainer()
        {
            var c = new CompositionContainer(new AggregateCatalog(parent.Catalog), parent);
            //RegisterAssembly(c, typeof(Nancy.INancyEngine).Assembly);
            RegisterAssembly(c, typeof(Nancy.Hosting.Self.NancyHost).Assembly);
            return c;
        }

        /// <summary>
        /// Registers any Nancy implementations in the given assembly.
        /// </summary>
        /// <param name="assembly"></param>
        public void RegisterAssembly(CompositionContainer container, Assembly assembly)
        {
            // scan for all types that derive from one of the Nancy interfaces
            var b = new RegistrationBuilder();
            foreach (var i in interfaces)
                foreach (var t in assembly.GetTypes())
                    if (i.IsAssignableFrom(t))
                        b.ForType(t)
                            .Export(x => x.AsContractType(i));

            ((AggregateCatalog)container.Catalog).Catalogs.Add(new AssemblyCatalog(assembly, b));
        }

        /// <summary>
        /// Bind the bootstrapper's implemented types into the container.
        /// This is necessary so a user can pass in a populated container but not have
        /// to take the responsibility of registering things like <see cref="INancyModuleCatalog"/> manually.
        /// </summary>
        /// <param name="applicationContainer">Application container to register into</param>
        protected override sealed void RegisterBootstrapperTypes(CompositionContainer applicationContainer)
        {
            applicationContainer.ComposeExportedValue<INancyModuleCatalog>(this);
        }

        /// <summary>
        /// Bind the default implementations of internally used types into the container as singletons
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="typeRegistrations">Type registrations to register</param>
        protected override sealed void RegisterTypes(CompositionContainer container, IEnumerable<TypeRegistration> typeRegistrations)
        {
            var t = new List<Type>();
            var b = new RegistrationBuilder();
            foreach (var typeRegistration in typeRegistrations)
            {
                t.Add(typeRegistration.ImplementationType);
                t.Add(typeRegistration.RegistrationType);
                b.ForType(typeRegistration.ImplementationType)
                    .SelectConstructor(x => x[0], (x, y) =>
                    {
                        var importManyType = x.ParameterType;
                        if (x.ParameterType.IsGenericType() &&
                            x.ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                            y.AsMany(true);
                        if (x.ParameterType.IsArray)
                            y.AsMany(true);
                    })
                    .Export(x => x.AsContractType(typeRegistration.RegistrationType));
            }

            ((AggregateCatalog)container.Catalog).Catalogs.Add(new TypeCatalog(t, b));
        }

        /// <summary>
        /// Bind the various collections into the container as singletons to later be resolved
        /// by IEnumerable{Type} constructor dependencies.
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="collectionTypeRegistrations">Collection type registrations to register</param>
        protected override sealed void RegisterCollectionTypes(CompositionContainer container, IEnumerable<CollectionTypeRegistration> collectionTypeRegistrations)
        {
            var t = new List<Type>();
            var b = new RegistrationBuilder();
            foreach (var collectionTypeRegistration in collectionTypeRegistrations)
            {
                t.Add(collectionTypeRegistration.RegistrationType);
                foreach (var implementationType in collectionTypeRegistration.ImplementationTypes)
                {
                    t.Add(implementationType);
                    b.ForType(implementationType)
                        .SelectConstructor(x => x[0])
                        .Export(x => x.AsContractType(collectionTypeRegistration.RegistrationType));
                }
            }

            ((AggregateCatalog)container.Catalog).Catalogs.Add(new TypeCatalog(t, b));
        }

        /// <summary>
        /// Bind the given instances into the container
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="instanceRegistrations">Instance registration types</param>
        protected override void RegisterInstances(CompositionContainer container, IEnumerable<InstanceRegistration> instanceRegistrations)
        {
            foreach (var instanceRegistration in instanceRegistrations)
                container.ComposeExportedValue(instanceRegistration.RegistrationType, instanceRegistration.Implementation);
        }

        /// <summary>
        /// Gets the diagnostics for intialisation
        /// </summary>
        /// <returns>An <see cref="IDiagnostics"/> implementation</returns>
        protected override IDiagnostics GetDiagnostics()
        {
            return ApplicationContainer.GetExportedValueOrDefault<IDiagnostics>();
        }

        /// <summary>
        /// Gets all registered application startup tasks
        /// </summary>
        /// <returns>An <see cref="System.Collections.Generic.IEnumerable{T}"/> instance containing <see cref="IApplicationStartup"/> instances. </returns>
        protected override IEnumerable<IApplicationStartup> GetApplicationStartupTasks()
        {
            return ApplicationContainer.GetExportedValues<IApplicationStartup>();
        }

        /// <summary>
        /// Gets all registered application registration tasks
        /// </summary>
        /// <returns>An <see cref="System.Collections.Generic.IEnumerable{T}"/> instance containing <see cref="IApplicationRegistrations"/> instances.</returns>
        protected override IEnumerable<IApplicationRegistrations> GetApplicationRegistrationTasks()
        {
            return ApplicationContainer.GetExportedValues<IApplicationRegistrations>();
        }

        /// <summary>
        /// Get INancyEngine
        /// </summary>
        /// <returns>An <see cref="INancyEngine"/> implementation</returns>
        protected override sealed INancyEngine GetEngineInternal()
        {
            return ApplicationContainer.GetExportedValueOrDefault<INancyEngine>();
        }

        /// <summary>
        /// Retrieve all module instances from the container
        /// </summary>
        /// <param name="container">Container to use</param>
        /// <returns>Collection of <see cref="INancyModule"/> instances</returns>
        protected override sealed IEnumerable<INancyModule> GetAllModules(CompositionContainer container)
        {
            return container.GetExportedValues<INancyModule>();
        }

        /// <summary>
        /// Retreive a specific module instance from the container
        /// </summary>
        /// <param name="container">Container to use</param>
        /// <param name="moduleType">Type of the module</param>
        /// <returns>An <see cref="INancyModule"/> instance</returns>
        protected override INancyModule GetModule(CompositionContainer container, Type moduleType)
        {
            return container.GetExportedValueOrDefault<INancyModule>(moduleType);
        }

        protected override CompositionContainer CreateRequestContainer()
        {
            return new CompositionContainer(new AggregateCatalog(ApplicationContainer.Catalog), ApplicationContainer);
        }

        protected override void RegisterRequestContainerModules(CompositionContainer container, IEnumerable<ModuleRegistration> moduleRegistrationTypes)
        {
            var t = new List<Type>();
            var b = new RegistrationBuilder();
            foreach (var moduleRegistrationType in moduleRegistrationTypes)
            {
                t.Add(moduleRegistrationType.ModuleType);
                b.ForType(moduleRegistrationType.ModuleType)
                    .SelectConstructor(i => i[0])
                    .Export<INancyModule>();
            }

            ((AggregateCatalog)container.Catalog).Catalogs.Add(new TypeCatalog(t, b));
        }

    }


}
