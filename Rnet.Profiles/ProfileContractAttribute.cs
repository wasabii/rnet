using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;

namespace Rnet.Profiles
{

    /// <summary>
    /// Indicates that an interface defines a profile contract.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    [MetadataAttribute]
    public sealed class ProfileContractAttribute : ExportAttribute, IProfileContractMetadata
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="ns"></param>
        /// <param name="name"></param>
        public ProfileContractAttribute(string ns, string name)
            : base(typeof(IProfile).FullName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(ns));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(name));

            Namespace = ns;
            Name = name;
        }

        /// <summary>
        /// Name of the profile.
        /// </summary>
        public string Namespace { get; private set; }

        /// <summary>
        /// Name of the profile.
        /// </summary>
        public string Name { get; private set; }

    }

}
