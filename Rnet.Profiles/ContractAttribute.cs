using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;

namespace Rnet.Profiles
{

    /// <summary>
    /// Provides metadata regarding a profile contract.
    /// </summary>
    public interface IContractMetadata
    {

        /// <summary>
        /// Namespace of the profile.
        /// </summary>
        string Namespace { get; }

        /// <summary>
        /// Name of the profile.
        /// </summary>
        string Name { get; }


    }

    /// <summary>
    /// Indicates that an interface defines a profile contract.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    [MetadataAttribute]
    public sealed class ContractAttribute : ExportAttribute, IContractMetadata
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="ns"></param>
        /// <param name="name"></param>
        public ContractAttribute(string ns, string name)
            : base(typeof(IProfile).FullName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(ns));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(name));

            Namespace = ns;
            Name = name;
        }

        /// <summary>
        /// Namespace of the profile.
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Name of the profile.
        /// </summary>
        public string Name { get; set; }

    }

}
