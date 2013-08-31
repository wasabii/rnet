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

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public T GetValue<T>(object instance)
        {
            Contract.Requires<ArgumentNullException>(instance != null);
            Contract.Requires<ArgumentException>(PropertyInfo.DeclaringType.IsInstanceOfType(instance));
            return (T)PropertyInfo.GetValue(instance);
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public object GetValue(object instance)
        {
            Contract.Requires<ArgumentNullException>(instance != null);
            Contract.Requires<ArgumentException>(PropertyInfo.DeclaringType.IsInstanceOfType(instance));
            return PropertyInfo.GetValue(instance);
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public void SetValue<T>(object instance, T value)
        {
            Contract.Requires<ArgumentNullException>(instance != null);
            Contract.Requires<ArgumentException>(PropertyInfo.DeclaringType.IsInstanceOfType(instance));
            PropertyInfo.SetValue(instance, value);
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public void SetValue(object instance, object value)
        {
            Contract.Requires<ArgumentNullException>(instance != null);
            Contract.Requires<ArgumentException>(PropertyInfo.DeclaringType.IsInstanceOfType(instance));
            PropertyInfo.SetValue(instance, value);
        }

    }

}
