using System;
using System.Diagnostics.Contracts;

using Rnet.Profiles.Metadata;

namespace Rnet.Drivers
{

    /// <summary>
    /// Provides a profile implementation and the metadata associated with it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Profile<T> : Profile
        where T : class
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="contract"></param>
        /// <param name="instance"></param>
        public Profile(RnetBusObject target, ProfileDescriptor contract, T instance)
            : base(target, contract, instance)
        {
            Contract.Requires(target != null);
            Contract.Requires(contract != null);
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
    public abstract class Profile
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="metadata"></param>
        /// <param name="instance"></param>
        internal protected Profile(RnetBusObject target, ProfileDescriptor metadata, object instance)
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

    }

}
