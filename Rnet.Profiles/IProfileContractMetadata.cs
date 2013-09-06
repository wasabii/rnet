namespace Rnet.Profiles
{

    /// <summary>
    /// Provides metadata regarding a profile contract.
    /// </summary>
    public interface IProfileContractMetadata
    {

        /// <summary>
        /// Name of the profile.
        /// </summary>
        string Namespace { get; }

        /// <summary>
        /// Name of the profile.
        /// </summary>
        string Name { get; }

    }

}
