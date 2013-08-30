using System;

namespace Rnet.Profiles
{

    /// <summary>
    /// Indicates a property is to be exposed as a value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ValueAttribute : Attribute
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="name"></param>
        public ValueAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Name of the value.
        /// </summary>
        public string Name { get; set; }

    }

}
