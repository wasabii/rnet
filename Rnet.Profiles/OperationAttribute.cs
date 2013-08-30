using System;

namespace Rnet.Profiles
{

    /// <summary>
    /// Indicates a method is to be exposed as an operation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class OperationAttribute : Attribute
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="name"></param>
        public OperationAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Name of the operation.
        /// </summary>
        public string Name { get; set; }

    }

}
