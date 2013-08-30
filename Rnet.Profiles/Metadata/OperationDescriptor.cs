using System.Reflection;

namespace Rnet.Profiles.Metadata
{

    /// <summary>
    /// Describes an operation available on a profile contract.
    /// </summary>
    public sealed class OperationDescriptor
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="name"></param>
        internal OperationDescriptor(MethodInfo methodInfo, string name)
        {
            MethodInfo = methodInfo;
            Name = name;
        }

        /// <summary>
        /// Gets the <see cref="MethodInfo"/> described by this <see cref="OperationDescriptor"/>.
        /// </summary>
        public MethodInfo MethodInfo { get; private set; }

        /// <summary>
        /// Name of the operaton.
        /// </summary>
        public string Name { get; private set; }

    }

}
