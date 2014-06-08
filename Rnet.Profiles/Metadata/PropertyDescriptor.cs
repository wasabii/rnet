using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Rnet.Profiles.Metadata
{

    /// <summary>
    /// Describes a value available on a profile contract.
    /// </summary>
    public sealed class PropertyDescriptor
    {

        ProfileDescriptor profile;
        PropertyInfo propertyInfo;
        string name;
        Type type;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="profile"></param>
        internal PropertyDescriptor(ProfileDescriptor profile)
        {
            Contract.Requires<ArgumentNullException>(profile != null);

            this.profile = profile;
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(profile != null);

            Contract.Invariant(propertyInfo == null || propertyInfo.DeclaringType == profile.Contract);
            Contract.Invariant(propertyInfo == null || !string.IsNullOrWhiteSpace(name));
            Contract.Invariant(propertyInfo == null || type != null);
        }

        /// <summary>
        /// Associated <see cref="ProfileDescriptor"/>.
        /// </summary>
        public ProfileDescriptor Profile
        {
            get { return profile; }
        }

        /// <summary>
        /// Gets the <see cref="propertyInfo"/> described by this <see cref="PropertyDescriptor"/>.
        /// </summary>
        public PropertyInfo PropertyInfo
        {
            get { return propertyInfo; }
        }

        /// <summary>
        /// Name of the value.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets the type of the return value to be expected.
        /// </summary>
        public Type Type
        {
            get { return type; }
        }

        /// <summary>
        /// Loads the descriptor from the given property.
        /// </summary>
        /// <param name="propertyInfo"></param>
        internal void Load(PropertyInfo propertyInfo)
        {
            Contract.Requires<ArgumentNullException>(propertyInfo != null);
            Contract.Requires<InvalidCastException>(propertyInfo.DeclaringType == Profile.Contract);
            Contract.Ensures(propertyInfo != null);
            Contract.Ensures(!string.IsNullOrWhiteSpace(name));
            Contract.Ensures(type != null);

            this.propertyInfo = propertyInfo;

            name = propertyInfo.Name;
            type = propertyInfo.PropertyType;

            LoadDataAnnotations();
        }

        /// <summary>
        /// Loads information from any data annotations.
        /// </summary>
        void LoadDataAnnotations()
        {

        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public T GetValue<T>(object instance)
        {
            Contract.Requires<ArgumentNullException>(instance != null);
            Contract.Requires<InvalidCastException>(typeof(T).IsAssignableFrom(Type));

            return (T)GetValue(instance);
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public object GetValue(object instance)
        {
            Contract.Requires<ArgumentNullException>(instance != null);
            Contract.Requires<InvalidCastException>(Profile.Contract.IsInstanceOfType(instance));
            Contract.Requires<InvalidCastException>(PropertyInfo.DeclaringType.IsInstanceOfType(instance));

            return propertyInfo.GetValue(instance);
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
            Contract.Requires<InvalidCastException>(Type.IsAssignableFrom(typeof(T)));

            SetValue(instance, (object)value);
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
            Contract.Requires<InvalidCastException>(Profile.Contract.IsInstanceOfType(instance));
            Contract.Requires<InvalidCastException>(PropertyInfo.DeclaringType.IsInstanceOfType(instance));

            propertyInfo.SetValue(instance, value);
        }

    }

}
