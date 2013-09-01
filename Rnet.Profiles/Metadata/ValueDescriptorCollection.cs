using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Rnet.Profiles.Metadata
{

    public sealed class ValueDescriptorCollection : IEnumerable<ValueDescriptor>
    {

        readonly Dictionary<string, ValueDescriptor> values =
            new Dictionary<string, ValueDescriptor>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        internal ValueDescriptorCollection()
        {

        }

        /// <summary>
        /// Gets the <see cref="ValueDescriptor"/> for the given property.
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public ValueDescriptor this[PropertyInfo propertyInfo]
        {
            get
            {
                Contract.Requires<ArgumentNullException>(propertyInfo != null);
                return this[propertyInfo.Name]; 
            }
        }

        /// <summary>
        /// Gets the <see cref="ValueDescriptor"/> for the given property.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public ValueDescriptor this[string propertyName]
        {
            get
            {
                Contract.Requires<ArgumentNullException>(propertyName != null);
                ValueDescriptor value;
                return values.TryGetValue(propertyName, out value) ? value : null;
            }
        }

        /// <summary>
        /// Adds the descriptor.
        /// </summary>
        /// <param name="descriptor"></param>
        internal void Add(ValueDescriptor descriptor)
        {
            Contract.Requires<ArgumentNullException>(descriptor != null);
            values[descriptor.PropertyInfo.Name] = descriptor;
        }

        /// <summary>
        /// Removes the descriptor.
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        internal bool Remove(ValueDescriptor descriptor)
        {
            Contract.Requires<ArgumentNullException>(descriptor != null);
            return values.Remove(descriptor.PropertyInfo.Name);
        }

        public IEnumerator<ValueDescriptor> GetEnumerator()
        {
            return values.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

}
