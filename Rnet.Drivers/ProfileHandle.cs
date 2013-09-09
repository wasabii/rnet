using System;
using System.Diagnostics.Contracts;

using Rnet.Profiles.Metadata;

namespace Rnet.Drivers
{

    /// <summary>
    /// Provides a profile implementation and the metadata associated with it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ProfileHandle<T> : ProfileHandle
        where T : class
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="profile"></param>
        /// <param name="instance"></param>
        public ProfileHandle(RnetBusObject target, ProfileDescriptor profile, T instance)
            : base(target, profile, instance)
        {
            Contract.Requires(target != null);
            Contract.Requires(profile != null);
            Contract.Requires(instance != null);
        }

        /// <summary>
        /// Gets the instance which provides the implementation of the profile.
        /// </summary>
        public new T Instance
        {
            get { return (T)base.Instance; }
        }

    }

    /// <summary>
    /// Provides a profile implementation and the metadata associated with it.
    /// </summary>
    public abstract class ProfileHandle
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="metadata"></param>
        /// <param name="instance"></param>
        internal protected ProfileHandle(RnetBusObject target, ProfileDescriptor metadata, object instance)
        {
            Contract.Requires<ArgumentNullException>(target != null);
            Contract.Requires<ArgumentNullException>(metadata != null);
            Contract.Requires<ArgumentNullException>(instance != null);

            Target = target;
            Metadata = metadata;
            Instance = instance;
        }

        /// <summary>
        /// Gets the bus object which is targetted by the profile.
        /// </summary>
        public RnetBusObject Target { get; private set; }

        /// <summary>
        /// Gets the metadata that describes the contract of the profile.
        /// </summary>
        public ProfileDescriptor Metadata { get; private set; }

        /// <summary>
        /// Gets the instance which provides the implementation of the profile.
        /// </summary>
        public object Instance { get; private set; }

        /// <summary>
        /// Gets a handle to the specified property.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public ProfilePropertyHandle this[PropertyDescriptor property]
        {
            get { Contract.Requires<ArgumentNullException>(property != null); return new ProfilePropertyHandle(this, property); }
        }

        /// <summary>
        /// Gets a handle to the specified command.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public ProfileCommandHandle this[CommandDescriptor command]
        {
            get { Contract.Requires<ArgumentNullException>(command != null); return new ProfileCommandHandle(this, command); }
        }

    }

}
