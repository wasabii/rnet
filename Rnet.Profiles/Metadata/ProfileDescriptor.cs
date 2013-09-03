using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Rnet.Profiles.Metadata
{

    /// <summary>
    /// Describes a profile that may be attached to an <see cref="RnetBusObject"/>.
    /// </summary>
    public sealed class ProfileDescriptor
    {

        public const string PROFILE_XMLNS_PREFIX = "urn:rnet:profiles::";
        public const string PROFILE_METADATA_XMLNS = "urn:rnet:profiles:metadata";

        Type contract;
        string id;
        string ns;
        string name;
        PropertyDescriptorCollection values;
        CommandDescriptorCollection operations;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ProfileDescriptor()
        {
            values = new PropertyDescriptorCollection();
            operations = new CommandDescriptorCollection();
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            System.Diagnostics.Contracts.Contract.Invariant(values != null);
            System.Diagnostics.Contracts.Contract.Invariant(operations != null);

            System.Diagnostics.Contracts.Contract.Invariant(contract == null || !string.IsNullOrWhiteSpace(id));
            System.Diagnostics.Contracts.Contract.Invariant(contract == null || !string.IsNullOrWhiteSpace(ns));
            System.Diagnostics.Contracts.Contract.Invariant(contract == null || !string.IsNullOrWhiteSpace(name));
        }

        /// <summary>
        /// Interface which provides the profile contract.
        /// </summary>
        public Type Contract
        {
            get { return contract; }
        }

        /// <summary>
        /// Full name of the profile. Generally appears in the URL.
        /// </summary>
        public string Id
        {
            get { return id; }
        }

        /// <summary>
        /// Namespace of the profile.
        /// </summary>
        public string Namespace
        {
            get { return ns; }
        }

        /// <summary>
        /// Name of the profile.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets the XML namespace for the profile.
        /// </summary>
        public XNamespace XmlNamespace
        {
            get { return PROFILE_XMLNS_PREFIX + id; }
        }

        /// <summary>
        /// Gets the set of <see cref="PropertyDescriptor"/>s that describe available values on the contract.
        /// </summary>
        public PropertyDescriptorCollection Properties
        {
            get { return values; }
        }

        /// <summary>
        /// Gets the set of <see cref="CommandDescriptor"/>s that describe available operations on the contract.
        /// </summary>
        public CommandDescriptorCollection Operations
        {
            get { return operations; }
        }

        /// <summary>
        /// Loads the profile from the given type.
        /// </summary>
        /// <param name="contract"></param>
        internal void Load(Type contract)
        {
            System.Diagnostics.Contracts.Contract.Requires<ArgumentNullException>(contract != null);
            System.Diagnostics.Contracts.Contract.Requires<ArgumentException>(contract.IsInterface);
            System.Diagnostics.Contracts.Contract.Ensures(!string.IsNullOrWhiteSpace(id));
            System.Diagnostics.Contracts.Contract.Ensures(!string.IsNullOrWhiteSpace(ns));
            System.Diagnostics.Contracts.Contract.Ensures(!string.IsNullOrWhiteSpace(name));

            this.contract = contract;

            // configure from attribute
            var attr = contract.GetCustomAttribute<ProfileContractAttribute>();
            if (attr == null)
                return;

            id = attr.Namespace + "." + attr.Name;
            ns = attr.Namespace;
            name = attr.Name;

            LoadDataAnnotations();
            LoadValues();
            LoadOperations();
        }

        /// <summary>
        /// Loads information from the Data Annotations attributes.
        /// </summary>
        void LoadDataAnnotations()
        {

        }

        /// <summary>
        /// Loads values from the contract.
        /// </summary>
        void LoadValues()
        {
            foreach (var property in contract.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                LoadValue(property);
        }

        /// <summary>
        /// Loads the value from the specified <see cref="PropertyInfo"/>.
        /// </summary>
        /// <param name="property"></param>
        void LoadValue(PropertyInfo property)
        {
            System.Diagnostics.Contracts.Contract.Requires(property != null);

            var descriptor = values[property];
            if (descriptor == null)
                descriptor = new PropertyDescriptor(this);

            descriptor.Load(property);
            values.Remove(descriptor);
            values.Add(descriptor);
        }

        /// <summary>
        /// Loads operations from the contract.
        /// </summary>
        void LoadOperations()
        {
            foreach (var method in contract.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                if (!method.IsSpecialName)
                    LoadOperation(method);
        }

        /// <summary>
        /// Loads the operation from the specified <see cref="MethodInfo"/>.
        /// </summary>
        /// <param name="method"></param>
        void LoadOperation(MethodInfo method)
        {
            System.Diagnostics.Contracts.Contract.Requires(method != null);

            var descriptor = operations[method];
            if (descriptor == null)
                descriptor = new CommandDescriptor(this);

            descriptor.Load(method);
            operations.Remove(descriptor);
            operations.Add(descriptor);
        }

        public XDocument ToXml()
        {
            // default namespace of profile
            var ns = (XNamespace)PROFILE_METADATA_XMLNS;

            // build XML document out of properties
            var xml = new XDocument(
                new XElement(ns + "Profile",
                    new XElement(ns + "Id", id),
                    new XElement(ns + "Namespace", ns),
                    new XElement(ns + "Name", name),
                    new XElement(ns + "XmlNamespace", PROFILE_XMLNS_PREFIX + id),
                    new XElement(ns + "XmlElementName", Name),
                    new XElement(ns + "Contract", Contract.FullName),
                    new XElement(ns + "Properties",
                        values.Select(i =>
                            new XElement(ns + "Property",
                                i.Name))),
                    new XElement(ns + "Commands",
                        operations.Select(i =>
                            new XElement(ns + "Command",
                                i.Name)))));

            return xml;
        }

        public XDocument ToXsd()
        {
            throw new NotImplementedException();
        }

    }

}
