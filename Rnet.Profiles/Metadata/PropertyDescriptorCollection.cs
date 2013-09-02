using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Rnet.Profiles.Metadata
{

    public sealed class PropertyDescriptorCollection : IEnumerable<PropertyDescriptor>
    {

        readonly Dictionary<string, PropertyDescriptor> properties =
            new Dictionary<string, PropertyDescriptor>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        internal PropertyDescriptorCollection()
        {

        }

        /// <summary>
        /// Gets the <see cref="PropertyDescriptor"/> for the given property.
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public PropertyDescriptor this[PropertyInfo propertyInfo]
        {
            get
            {
                Contract.Requires<ArgumentNullException>(propertyInfo != null);
                return this[propertyInfo.Name]; 
            }
        }

        /// <summary>
        /// Gets the <see cref="PropertyDescriptor"/> for the given property.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public PropertyDescriptor this[string propertyName]
        {
            get
            {
                Contract.Requires<ArgumentNullException>(propertyName != null);
                PropertyDescriptor property;
                return properties.TryGetValue(propertyName, out property) ? property : null;
            }
        }

        /// <summary>
        /// Adds the descriptor.
        /// </summary>
        /// <param name="descriptor"></param>
        internal void Add(PropertyDescriptor descriptor)
        {
            Contract.Requires<ArgumentNullException>(descriptor != null);
            properties[descriptor.PropertyInfo.Name] = descriptor;
        }

        /// <summary>
        /// Removes the descriptor.
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        internal bool Remove(PropertyDescriptor descriptor)
        {
            Contract.Requires<ArgumentNullException>(descriptor != null);
            return properties.Remove(descriptor.PropertyInfo.Name);
        }

        public IEnumerator<PropertyDescriptor> GetEnumerator()
        {
            return properties.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

}
