using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Rnet.Profiles.Metadata
{

    public sealed class OperationDescriptorCollection : IEnumerable<OperationDescriptor>
    {

        Dictionary<string, OperationDescriptor> items =
            new Dictionary<string, OperationDescriptor>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="type"></param>
        internal OperationDescriptorCollection(Type type)
        {
            items = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Select(i => new { Method = i, Attribute = i.GetCustomAttribute<OperationAttribute>() })
                .Where(i => i.Attribute != null)
                .ToDictionary(i => i.Attribute.Name, i => new OperationDescriptor(i.Method, i.Attribute.Name));
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
                return items[methodInfo.Name];
            }
        }

        /// <summary>
        /// Gets the <see cref="OperationDescriptor"/> for the given method.
        /// </summary>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public OperationDescriptor this[string methodName]
        {
            get { return items[methodName]; }
        }

        public IEnumerator<OperationDescriptor> GetEnumerator()
        {
            return items.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

}
