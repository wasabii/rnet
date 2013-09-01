using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.ServiceModel;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Rnet.Profiles.Metadata
{

    /// <summary>
    /// Describes a profile that may be attached to an <see cref="RnetBusObject"/>.
    /// </summary>
    public sealed class ProfileDescriptor
    {

        const string UNKNOWN_ID_PREFIX = "unknown.";
        const string DEFAULT_NS_PREFIX = "urn:rnet:profiles::";

        Type contract;
        string id;
        XName xmlName;
        ValueDescriptorCollection values;
        OperationDescriptorCollection operations;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ProfileDescriptor()
        {
            values = new ValueDescriptorCollection();
            operations = new OperationDescriptorCollection();
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            System.Diagnostics.Contracts.Contract.Invariant(values != null);
            System.Diagnostics.Contracts.Contract.Invariant(operations != null);

            System.Diagnostics.Contracts.Contract.Invariant(contract == null || !string.IsNullOrWhiteSpace(id));
            System.Diagnostics.Contracts.Contract.Invariant(contract == null || xmlName != null);
        }

        /// <summary>
        /// Interface which provides the profile contract.
        /// </summary>
        public Type Contract
        {
            get { return contract; }
        }

        /// <summary>
        /// Full name of the profile.
        /// </summary>
        public string Id
        {
            get { return id; }
        }

        /// <summary>
        /// Element name to use when returning profile information as XML.
        /// </summary>
        public XName XmlName
        {
            get { return xmlName; }
        }

        /// <summary>
        /// Gets the set of <see cref="ValueDescriptor"/>s that describe available values on the contract.
        /// </summary>
        public ValueDescriptorCollection Values
        {
            get { return values; }
        }

        /// <summary>
        /// Gets the set of <see cref="OperationDescriptor"/>s that describe available operations on the contract.
        /// </summary>
        public OperationDescriptorCollection Operations
        {
            get { return operations; }
        }

        /// <summary>
        /// Loads the profile from the given type.
        /// </summary>
        /// <param name="type"></param>
        internal void Load(Type type)
        {
            System.Diagnostics.Contracts.Contract.Requires<ArgumentNullException>(type != null);
            System.Diagnostics.Contracts.Contract.Requires<ArgumentException>(type.IsInterface);

            contract = type;

            LoadType();
            LoadProfileContract();
            LoadServiceContract();
            LoadXmlAttributes();
            LoadDataAnnotations();

            LoadValues();
            LoadOperations();
        }

        /// <summary>
        /// Loads information from the type itself.
        /// </summary>
        void LoadType()
        {
            System.Diagnostics.Contracts.Contract.Requires<ArgumentNullException>(contract != null);
            System.Diagnostics.Contracts.Contract.Ensures(!string.IsNullOrWhiteSpace(id));
            System.Diagnostics.Contracts.Contract.Ensures(xmlName != null && !string.IsNullOrWhiteSpace(xmlName.LocalName));

            id = UNKNOWN_ID_PREFIX + contract.Name;
            xmlName = XName.Get(contract.Name, DEFAULT_NS_PREFIX + id);
        }

        /// <summary>
        /// Loads information from the <see cref="ProfileContractAttribute"/>.
        /// </summary>
        void LoadProfileContract()
        {
            var attr = contract.GetCustomAttribute<ProfileContractAttribute>();
            if (attr == null)
                return;

            id = attr.Id;
            xmlName = XName.Get(xmlName.LocalName, DEFAULT_NS_PREFIX + id);
        }

        /// <summary>
        /// Loads information from the ServiceContract attributes.
        /// </summary>
        void LoadServiceContract()
        {
            var attr = contract.GetCustomAttribute<ServiceContractAttribute>();
            if (attr == null)
                return;

            if (!string.IsNullOrWhiteSpace(attr.Name))
                xmlName = XName.Get(attr.Name, xmlName.NamespaceName);
            if (!string.IsNullOrWhiteSpace(attr.Namespace))
                xmlName = XName.Get(xmlName.LocalName, attr.Namespace);
        }

        /// <summary>
        /// Loads information from the XML serialization attributes.
        /// </summary>
        void LoadXmlAttributes()
        {
            var attr = contract.GetCustomAttribute<XmlRootAttribute>();
            if (attr == null)
                return;

            if (!string.IsNullOrWhiteSpace(attr.ElementName))
                xmlName = XName.Get(attr.ElementName, xmlName.NamespaceName);
            if (!string.IsNullOrWhiteSpace(attr.Namespace))
                xmlName = XName.Get(xmlName.LocalName, attr.Namespace);
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
                descriptor = new ValueDescriptor(this);

            descriptor.Load(property);
            values.Remove(descriptor);
            if (!descriptor.Ignore)
                values.Add(descriptor);
        }

        /// <summary>
        /// Loads operations from the contract.
        /// </summary>
        void LoadOperations()
        {
            foreach (var method in contract.GetMethods(BindingFlags.Public | BindingFlags.Instance))
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
                descriptor = new OperationDescriptor(this);

            descriptor.Load(method);
            operations.Remove(descriptor);
            operations.Add(descriptor);
        }

        public XDocument ToXsd()
        {
            throw new NotImplementedException();
        }

    }

}
