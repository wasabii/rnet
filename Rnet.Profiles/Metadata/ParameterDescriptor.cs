using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Rnet.Profiles.Metadata
{

    /// <summary>
    /// Describes a parameter of an operation.
    /// </summary>
    public sealed class ParameterDescriptor
    {

        OperationDescriptor operation;
        ParameterInfo parameterInfo;
        string name;
        Type type;
        int order;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="operation"></param>
        internal ParameterDescriptor(OperationDescriptor operation)
        {
            Contract.Requires<ArgumentNullException>(operation != null);

            this.operation = operation;
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(operation != null);

            Contract.Invariant(parameterInfo == null || parameterInfo.Member == operation.MethodInfo);
            Contract.Invariant(parameterInfo == null || !string.IsNullOrWhiteSpace(name));
            Contract.Invariant(parameterInfo == null || type != null);
        }

        /// <summary>
        /// Associated <see cref="Operation"/>.
        /// </summary>
        public OperationDescriptor Operation
        {
            get { return operation; }
        }

        /// <summary>
        /// Gets the <see cref="ParameterInfo"/> that correspondence to this descriptor.
        /// </summary>
        public ParameterInfo ParameterInfo
        {
            get { return parameterInfo; }
        }

        /// <summary>
        /// Name of the parameter.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Type of the parameter.
        /// </summary>
        public Type Type
        {
            get { return type; }
        }

        /// <summary>
        /// Gets the 0-based index of the parameter.
        /// </summary>
        public int Order
        {
            get { return order; }
        }

        /// <summary>
        /// Loads the descriptor from the given <see cref="ParameterInfo"/>.
        /// </summary>
        /// <param name="parameter"></param>
        internal void Load(ParameterInfo parameter)
        {
            Contract.Requires<ArgumentNullException>(parameter != null);
            Contract.Requires<ArgumentException>(parameter.Member == Operation.MethodInfo);
            Contract.Ensures(parameterInfo != null);
            Contract.Ensures(name != null);
            Contract.Ensures(type != null);

            parameterInfo = parameter;
            LoadParameterInfo();
        }

        /// <summary>
        /// Loads information from the method itself.
        /// </summary>
        void LoadParameterInfo()
        {
            Contract.Requires(parameterInfo != null);
            Contract.Ensures(name != null);
            Contract.Ensures(type != null);

            name = parameterInfo.Name;
            type = parameterInfo.ParameterType;
            order = parameterInfo.Position;
        }

    }

}
