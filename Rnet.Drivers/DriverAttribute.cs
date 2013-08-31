using System;
using System.ComponentModel.Composition;

namespace Rnet.Drivers.Russound
{

    /// <summary>
    /// Exposes a <see cref="Driver"/> implementation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [MetadataAttribute]
    public class DriverAttribute : ExportAttribute
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public DriverAttribute()
            : base(typeof(Driver))
        {

        }

    }

}
