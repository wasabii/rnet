using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Rnet.Profiles.Metadata
{

    public sealed class OperationDescriptorCollection : IEnumerable<OperationDescriptor>
    {

        readonly Dictionary<string, OperationDescriptor> operations =
            new Dictionary<string, OperationDescriptor>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        internal OperationDescriptorCollection()
        {

        }

        /// <summary>
        /// Gets the <see cref="OperationDescriptor"/> for the given method.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public OperationDescriptor this[MethodInfo methodInfo]
        {
            get
            {
                Contract.Requires<ArgumentNullException>(methodInfo != null);
                return this[methodInfo.Name];
            }
        }

        /// <summary>
        /// Gets the <see cref="OperationDescriptor"/> for the given method.
        /// </summary>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public OperationDescriptor this[string methodName]
        {
            get
            {
                Contract.Requires<ArgumentNullException>(methodName != null);
                OperationDescriptor operation;
                return operations.TryGetValue(methodName, out operation) ? operation : null;
            }
        }

        /// <summary>
        /// Adds the descriptor.
        /// </summary>
        /// <param name="descriptor"></param>
        internal void Add(OperationDescriptor descriptor)
        {
            Contract.Requires<ArgumentNullException>(descriptor != null);
            operations[descriptor.MethodInfo.Name] = descriptor;
        }

        /// <summary>
        /// Removes the descriptor.
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        internal bool Remove(OperationDescriptor descriptor)
        {
            Contract.Requires<ArgumentNullException>(descriptor != null);
            return operations.Remove(descriptor.MethodInfo.Name);
        }

        public IEnumerator<OperationDescriptor> GetEnumerator()
        {
            return operations.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

}
