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

            this.methodInfo = methodInfo;

            name = methodInfo.Name;
        }

        /// <summary>
        /// Invokes the given command on the specified instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public object Invoke(object instance, params object[] args)
        {
            Contract.Requires<ArgumentNullException>(instance != null);
            Contract.Requires<InvalidCastException>(Profile.Contract.IsInstanceOfType(instance));

            return methodInfo.Invoke(instance, args);
        }

    }

}
