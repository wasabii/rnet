using System;

namespace Rnet.Drivers
{

    /// <summary>
    /// Describes a profile.
    /// </summary>
    public sealed class ProfileMetadata
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="ns"></param>
        /// <param name="name"></param>
        /// <param name="serviceContract"></param>
        internal ProfileMetadata(string ns, string name, Type serviceContract)
        {
            Namespace = ns;
            Name = name;
            ServiceContract = serviceContract;
        }

        /// <summary>
        /// Unique namespace of the profile.
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Unique name within the namespace of the profile.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Interface which provides the service contract.
        /// </summary>
        public Type ServiceContract { get; set; }

    }

}
