using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.Registration;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

using Nancy;
using Nancy.Bootstrapper;
using Nancy.Diagnostics;
using Nancy.Conventions;
using Nancy.Responses;
using Nancy.Serialization.JsonNet;
using Nancy.Responses.Negotiation;

namespace Rnet.Service
{

    public class NancyBootstrapper : NancyBootstrapperWithRequestContainerBase<CompositionContainer>
    {

        /// <summary>
        /// Base class so we can set the container easily.
        /// </summary>
        abstract class FuncFactory
        {

            /// <summary>
            /// Reference to the hosting container.
            /// </summary>
            public CompositionContainer Container { get; set; }

        }

        /// <summary>
        /// Provides a TinyIoC-like factory implementation for Func imports. MEF doesn't support resolving Funcs
        /// directly into a factory, so this class exports a Func signature and delays import of a MEF ExportFactory
        /// until its actually used.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        [Export]
        class FuncFactory<T> : FuncFactory
        {

            [Export]
            class FuncFactoryFactory<T>
            {

                [Import]
                public ExportFactory<T> Factory { get; set; }

            }

            /// <summary>
            /// Implements the TinyIoC factory method.
            /// </summary>
            /// <returns></returns>
            [Export]
            public T CreateExport()
            {
                return Container.GetExportedValue<FuncFactoryFactory<T>>().Factory.CreateExport().Value;
            }

        }

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

        protected override Nancy.Bootstrapper.NancyInternalConfiguration InternalConfiguration
        {
            get
            {
                return NancyInternalConfiguration.WithOverrides(c =>
                {
                    c.Serializers.Remove(typeof(DefaultJsonSerializer));
                    c.Serializers.Insert(c.Serializers.Count, typeof(JsonNetSerializer));

                    c.ResponseProcessors.Remove(typeof(ViewProcessor));
                    c.ResponseProcessors.Remove(typeof(JsonProcessor));
                    c.ResponseProcessors.Insert(c.ResponseProcessors.Count, typeof(JsonProcessor));
                });
            }
        }

        /// <summary>
        /// Create a default, unconfigured, container
        /// </summary>
        /// <returns>Container instance</returns>
        protected override CompositionContainer GetApplicationContainer()
        {
            var c = new CompositionContainer(new AggregateCatalog(parent.Catalog), parent);
            RegisterAssembly(c, typeof(Nancy.Hosting.Self.NancyHost).Assembly);
            RegisterAssembly(c, typeof(Nancy.Serialization.JsonNet.JsonNetBodyDeserializer).Assembly);
            return c;
        }

        /// <summary>
        /// Registers the given types with the container.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="types"></param>
        /// <param name="builder"></param>
        RegistrationBuilder RegisterAssemblyWithBuilder(CompositionContainer container, Assembly assembly, RegistrationBuilder builder)
        {
            ((AggregateCatalog)container.Catalog).Catalogs.Add(new AssemblyCatalog(assembly, builder));
            return builder;
        }

        /// <summary>
        /// Registers the given types with the container.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="types"></param>
        /// <param name="builder"></param>
        RegistrationBuilder RegisterTypesWithBuilder(CompositionContainer container, IEnumerable<Type> types, RegistrationBuilder builder)
        {
            if (types.Any())
                ((AggregateCatalog)container.Catalog).Catalogs.Add(new TypeCatalog(types, builder));
            return builder;
        }

        /// <summary>
        /// Configures the <see cref="ImportBuilder"/> for the given parameter.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="b"></param>
        ImportBuilder BuildImportConstructorParameter(CompositionContainer container, ImportBuilder b, ParameterInfo p)
        {
            var name = p.ParameterType.FullName;

            Type importManyType = null;
            if (p.ParameterType.IsGenericType() &&
                p.ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                importManyType = p.ParameterType.GetGenericArguments()[0];
            if (p.ParameterType.IsArray)
                importManyType = p.ParameterType.GetElementType();
            if (importManyType != null)
            {
                b.AsMany(true);
                b.AsContractType(importManyType);
                return b;
            }

            Type funcType = null;
            if (p.ParameterType.IsGenericType() &&
                p.ParameterType.GetGenericTypeDefinition() == typeof(Func<>))
                funcType = p.ParameterType.GetGenericArguments()[0];

            if (funcType != null)
            {
                // FuncFactory exports Func and dispatches it to an ExportFactory composed once
                var t = typeof(FuncFactory<>).MakeGenericType(funcType);
                var f = (FuncFactory)container.GetExports(t, null, null).Select(i => i.Value).FirstOrDefault();
                if (f == null)
                {
                    f = (FuncFactory)Activator.CreateInstance(t);
                    f.Container = container;
                    container.ComposeParts(f);
                }

                // parameter contract type should now properly find the function
                b.AsMany(false);
                b.AsContractType(p.ParameterType);
                return b;
            }

            // fall back to normal method
            b.AsMany(false);
            b.AsContractType(p.ParameterType);
            return b;
        }

        /// <summary>
        /// Configures the <see cref="RegistrationBuilder"/> to export the given type under all of the supported
        /// contracts given.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="implementationType"></param>
        /// <param name="contractTypes"></param>
        RegistrationBuilder BuildTypeContracts(CompositionContainer container, RegistrationBuilder b, Type implementationType, IEnumerable<Type> contractTypes)
        {
            var p = b.ForType(implementationType)
                .SelectConstructor(x => x.Length > 0 ? x[0] : null, (x, y) => BuildImportConstructorParameter(container, y, x));
            foreach (var contractType in contractTypes)
                if (contractType.IsAssignableFrom(implementationType))
                    p.Export(x => x.AsContractType(contractType));
            return b;
        }

        /// <summary>
        /// Configures the <see cref="RegistrationBuilder"/> to export the given type under multiple contracts.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="implementationType"></param>
        /// <param name="contractTypes"></param>
        RegistrationBuilder BuildTypeContracts(CompositionContainer container, RegistrationBuilder b, Type implementationType, params Type[] contractTypes)
        {
            return BuildTypeContracts(container, b, implementationType, (IEnumerable<Type>)contractTypes);
        }

        /// <summary>
        /// Configures the <see cref="RegistrationBuilder"/> to export the given implementation types under any
        /// supported contract type that is specified.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="implementationTypes"></param>
        /// <param name="contractTypes"></param>
        /// <returns></returns>
        RegistrationBuilder BuildTypeContracts(CompositionContainer container, RegistrationBuilder b, IEnumerable<Type> implementationTypes, IEnumerable<Type> contractTypes)
        {
            foreach (var implementationType in implementationTypes)
                BuildTypeContracts(container, b, implementationType, contractTypes);
            return b;
        }

        /// <summary>
        /// Registers any Nancy implementations in the given assembly.
        /// </summary>
        /// <param name="assembly"></param>
        public void RegisterAssembly(CompositionContainer container, Assembly assembly)
        {
            Contract.Requires<ArgumentNullException>(container != null);
            Contract.Requires<ArgumentNullException>(assembly != null);

            RegisterAssemblyWithBuilder(container, assembly,
                BuildTypeContracts(container, new RegistrationBuilder(), assembly.GetTypes(), interfaces));
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
                BuildTypeContracts(container, b, typeRegistration.ImplementationType, typeRegistration.RegistrationType);
            }

            if (t.Count > 0)
                RegisterTypesWithBuilder(container, t, b);
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
                foreach (var implementationType in collectionTypeRegistration.ImplementationTypes)
                {
                    t.Add(implementationType);
                    BuildTypeContracts(container, b, implementationType, collectionTypeRegistration.RegistrationType);
                }
            }

            if (t.Count > 0)
                RegisterTypesWithBuilder(container, t, b);
        }

        

        /// <summary>
        /// Bind the given instances into the container
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="instanceRegistrations">Instance registration types</param>
        protected override void RegisterInstances(CompositionContainer container, IEnumerable<InstanceRegistration> instanceRegistrations)
        {
            var b = new CompositionBatch();
            foreach (var instanceRegistration in instanceRegistrations)
            {
                var m = new Dictionary<string, object> 
                {
                    { "ExportTypeIdentity", AttributedModelServices.GetTypeIdentity(instanceRegistration.Implementation.GetType()) }
                };
                b.AddExport(new Export(AttributedModelServices.GetContractName(instanceRegistration.RegistrationType), m, () => instanceRegistration.Implementation));
            }
            container.Compose(b);
        }

        /// <summary>
        /// Gets the diagnostics for intialisation
        /// </summary>
        /// <returns>An <see cref="IDiagnostics"/> implementation</returns>
        protected override IDiagnostics GetDiagnostics()
        {
            return ApplicationContainer.GetExportedValue<IDiagnostics>();
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
            return ApplicationContainer.GetExportedValue<INancyEngine>();
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
            var module = container.GetExportedValues<INancyModule>().FirstOrDefault(i => i.GetType() == moduleType);
            if (module == null)
                throw new Exception("Module not found.");

            return module;
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
                BuildTypeContracts(container, b, moduleRegistrationType.ModuleType, typeof(INancyModule));
            }

            RegisterTypesWithBuilder(container, t, b);
        }

    }


}
