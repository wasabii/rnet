using System;
using System.Diagnostics.Contracts;

namespace Rnet.Profiles.Metadata
{

    /// <summary>
    /// Describes a profile that may be attached to an <see cref="RnetBusObject"/>.
    /// </summary>
    public sealed class ProfileDescriptor
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="ns"></param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public ProfileDescriptor(string ns, string name, Type type)
        {
            System.Diagnostics.Contracts.Contract.Requires<ArgumentNullException>(ns != null);
            System.Diagnostics.Contracts.Contract.Requires<ArgumentNullException>(name != null);
            System.Diagnostics.Contracts.Contract.Requires<ArgumentNullException>(type != null);

            Namespace = ns;
            Name = name;
            Contract = type;
            ValueDescriptors = new ValueDescriptorCollection(type);
            OperationDescriptors = new OperationDescriptorCollection(type);
        }

        /// <summary>
        /// Unique namespace of the profile.
        /// </summary>
        public string Namespace { get; private set; }

        /// <summary>
        /// Unique name within the namespace of the profile.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Interface which provides the profile contract.
        /// </summary>
        public Type Contract { get; private set; }

        /// <summary>
        /// Gets the set of <see cref="ValueDescriptor"/>s that describe available values on the contract.
        /// </summary>
        public ValueDescriptorCollection ValueDescriptors { get; private set; }

        /// <summary>
        /// Gets the set of <see cref="OperationDescriptor"/>s that describe available operations on the contract.
        /// </summary>
        public OperationDescriptorCollection OperationDescriptors { get; private set; }

    }

}
