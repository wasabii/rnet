using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Rnet.Profiles.Metadata
{

    public sealed class ParameterDescriptorCollection : IEnumerable<ParameterDescriptor>
    {

        readonly Dictionary<string, ParameterDescriptor> parameters =
            new Dictionary<string, ParameterDescriptor>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        internal ParameterDescriptorCollection()
        {

        }

        /// <summary>
        /// Gets the <see cref="ParameterDescriptor"/> for the given parameter.
        /// </summary>
        /// <param name="parameterInfo"></param>
        /// <returns></returns>
        public ParameterDescriptor this[MethodInfo parameterInfo]
        {
            get
            {
                Contract.Requires<ArgumentNullException>(parameterInfo != null);
                return this[parameterInfo.Name];
            }
        }

        /// <summary>
        /// Gets the <see cref="ParameterDescriptor"/> for the given parameter.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public ParameterDescriptor this[string parameterName]
        {
            get
            {
                Contract.Requires<ArgumentNullException>(parameterName != null);
                ParameterDescriptor parameter;
                return parameters.TryGetValue(parameterName, out parameter) ? parameter : null;
            }
        }

        /// <summary>
        /// Adds the descriptor.
        /// </summary>
        /// <param name="descriptor"></param>
        internal void Add(ParameterDescriptor descriptor)
        {
            Contract.Requires<ArgumentNullException>(descriptor != null);
            parameters[descriptor.ParameterInfo.Name] = descriptor;
        }

        /// <summary>
        /// Removes the descriptor.
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        internal bool Remove(ParameterDescriptor descriptor)
        {
            Contract.Requires<ArgumentNullException>(descriptor != null);
            return parameters.Remove(descriptor.ParameterInfo.Name);
        }

        public IEnumerator<ParameterDescriptor> GetEnumerator()
        {
            return parameters.Values.OrderBy(i => i.Order).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

}
