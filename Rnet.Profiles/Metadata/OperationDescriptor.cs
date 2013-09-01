using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.ServiceModel;

namespace Rnet.Profiles.Metadata
{

    /// <summary>
    /// Describes an operation available on a profile.
    /// </summary>
    public sealed class OperationDescriptor
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
        internal OperationDescriptor(ProfileDescriptor profile)
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
        /// <param name="method"></param>
        internal void Load(MethodInfo method)
        {
            Contract.Requires<ArgumentNullException>(method != null);
            Contract.Requires<ArgumentNullException>(method.DeclaringType == Profile.Contract);
            Contract.Ensures(methodInfo != null);
            Contract.Ensures(name != null);
            Contract.Ensures(type != null);

            methodInfo = method;
            LoadMethodInfo();
            LoadOperationContract();
        }

        /// <summary>
        /// Loads information from the method itself.
        /// </summary>
        void LoadMethodInfo()
        {
            Contract.Requires(methodInfo != null);
            Contract.Ensures(name != null);
            Contract.Ensures(type != null);

            name = methodInfo.Name;
            type = methodInfo.ReturnType;
        }

        /// <summary>
        /// Loads information from the <see cref="OperationContractAttribute"/>.
        /// </summary>
        void LoadOperationContract()
        {
            Contract.Requires(methodInfo != null);
            Contract.Ensures(name != null);

            var attr = methodInfo.GetCustomAttribute<OperationContractAttribute>();
            if (attr == null)
                return;

            if (!string.IsNullOrWhiteSpace(attr.Name))
                name = attr.Name;
        }

    }

}
