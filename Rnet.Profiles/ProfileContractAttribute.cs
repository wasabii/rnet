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
        /// Id of the profile.
        /// </summary>
        string Id { get; }

    }

    /// <summary>
    /// Indicates that an interface defines a profile contract.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    [MetadataAttribute]
    public sealed class ProfileContractAttribute : ExportAttribute, IContractMetadata
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="id"></param>
        public ProfileContractAttribute(string id)
            : base(typeof(IProfile).FullName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(id));

            Id = id;
        }

        /// <summary>
        /// Id of the profile.
        /// </summary>
        public string Id { get; private set; }

    }

}
