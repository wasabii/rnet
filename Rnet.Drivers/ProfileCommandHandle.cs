using System;
using System.Diagnostics.Contracts;

using Rnet.Profiles.Metadata;

namespace Rnet.Drivers
{

    /// <summary>
    /// Represents a handle to a command on an active profile.
    /// </summary>
    public sealed class ProfileCommandHandle
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="metadata"></param>
        public ProfileCommandHandle(ProfileHandle profile, CommandDescriptor metadata)
        {
            Contract.Requires<ArgumentNullException>(profile != null);
            Contract.Requires<ArgumentNullException>(metadata != null);

            Profile = profile;
            Metadata = metadata;
        }

        /// <summary>
        /// Profile to which we have a handle.
        /// </summary>
        public ProfileHandle Profile { get; private set; }

        /// <summary>
        /// Metadata of command.
        /// </summary>
        public CommandDescriptor Metadata { get; private set; }

    }

}
