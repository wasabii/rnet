using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Rnet.Profiles.Metadata
{

    public sealed class ValueDescriptorCollection : IEnumerable<ValueDescriptor>
    {

        Dictionary<string, ValueDescriptor> items =
            new Dictionary<string, ValueDescriptor>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="type"></param>
        internal ValueDescriptorCollection(Type type)
        {
            items = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(i => new { Property = i, Attribute = i.GetCustomAttribute<ValueAttribute>() })
                .Where(i => i.Attribute != null)
                .ToDictionary(i => i.Attribute.Name, i => new ValueDescriptor(i.Property, i.Attribute.Name));
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
                return items[propertyInfo.Name]; 
            }
        }

        /// <summary>
        /// Gets the <see cref="ValueDescriptor"/> for the given property.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public ValueDescriptor this[string propertyName]
        {
            get { return items[propertyName]; }
        }

        public IEnumerator<ValueDescriptor> GetEnumerator()
        {
            return items.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

}
