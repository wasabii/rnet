using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Rnet.Profiles.Metadata
{

    /// <summary>
    /// Describes an operation available on a profile.
    /// </summary>
    public sealed class CommandDescriptor
    {

        ProfileDescriptor profile;
        MethodInfo methodInfo;
        string name;
        Type type;
        ParameterDescriptorCollection parameters;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="profile"></param>
        internal CommandDescriptor(ProfileDescriptor profile)
        {
            Contract.Requires<ArgumentNullException>(profile != null);

            this.profile = profile;
            this.parameters = new ParameterDescriptorCollection();
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(profile != null);
            Contract.Invariant(parameters != null);

            Contract.Invariant(methodInfo == null || methodInfo.DeclaringType == profile.Contract);
            Contract.Invariant(methodInfo == null || !string.IsNullOrWhiteSpace(name));
            Contract.Invariant(methodInfo == null || type != null);
        }

        /// <summary>
        /// Associated <see cref="ProfileDescriptor"/>.
        /// </summary>
        public ProfileDescriptor Profile
        {
            get { return profile; }
        }

        /// <summary>
        /// Gets the <see cref="MethodInfo"/> to be executed to carry out the operation.
        /// </summary>
        public MethodInfo MethodInfo
        {
            get { return methodInfo; }
        }

        /// <summary>
        /// Name of the operaton.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Type returned from the operation.
        /// </summary>
        public Type Type
        {
            get { return type; }
        }

        /// <summary>
        /// Parameters to be passed to the operation.
        /// </summary>
        public ParameterDescriptorCollection Parameters
        {
            get { return parameters; }
        }

        /// <summary>
        /// Loads the descriptor from the given method.
        /// </summary>
        /// <param name="methodInfo"></param>
        internal void Load(MethodInfo methodInfo)
        {
            Contract.Requires<ArgumentNullException>(methodInfo != null);
            Contract.Requires<ArgumentNullException>(methodInfo.DeclaringType == Profile.Contract);
            Contract.Ensures(methodInfo != null);
            Contract.Ensures(name != null);
            Contract.Ensures(type != null);

            this.methodInfo = methodInfo;

            name = methodInfo.Name;
        }

    }

}
