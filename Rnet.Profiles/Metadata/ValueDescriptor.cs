using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Rnet.Profiles.Metadata
{

    /// <summary>
    /// Describes a value available on a profile contract.
    /// </summary>
    public sealed class ValueDescriptor
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="name"></param>
        internal ValueDescriptor(PropertyInfo propertyInfo, string name)
        {
            Contract.Requires<ArgumentNullException>(propertyInfo != null);
            Contract.Requires<ArgumentNullException>(name != null);

            PropertyInfo = propertyInfo;
            Name = name;
        }

        /// <summary>
        /// Gets the <see cref="PropertyInfo"/> described by this <see cref="ValueDescriptor"/>.
        /// </summary>
        public PropertyInfo PropertyInfo { get; private set; }

        /// <summary>
        /// Name of the value.
        /// </summary>
        public string Name { get; private set; }

    }

}
