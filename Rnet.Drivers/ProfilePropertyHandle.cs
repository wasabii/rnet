using System;
using System.Diagnostics.Contracts;

using Rnet.Profiles.Metadata;

namespace Rnet.Drivers
{

    /// <summary>
    /// Represents a handle to a property on an active profile.
    /// </summary>
    public sealed class ProfilePropertyHandle
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="property"></param>
        public ProfilePropertyHandle(ProfileHandle profile, PropertyDescriptor property)
        {
            Contract.Requires<ArgumentNullException>(profile != null);
            Contract.Requires<ArgumentNullException>(property != null);

            Profile = profile;
            Metadata = property;
        }

        /// <summary>
        /// Profile to which we have a handle.
        /// </summary>
        public ProfileHandle Profile { get; private set; }

        /// <summary>
        /// Metadata of property.
        /// </summary>
        public PropertyDescriptor Metadata { get; private set; }

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        /// <returns></returns>
        public object Get()
        {
            return Metadata.GetValue(Profile.Instance);
        }

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>()
        {
            return Metadata.GetValue<T>(Profile.Instance);
        }

        /// <summary>
        /// Sets the value of the property.
        /// </summary>
        /// <param name="value"></param>
        public void Set(object value)
        {
            Metadata.SetValue(Profile.Instance, value);
        }

        /// <summary>
        /// Sets the value of the property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        public void Set<T>(T value)
        {
            Metadata.SetValue<T>(Profile.Instance, value);
        }

    }

}
