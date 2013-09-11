using System;
using System.ComponentModel.Composition;

namespace Nancy.Bootstrapper.Mef
{

    /// <summary>
    /// Derived attribute version for exporting Nancy types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class NancyExportAttribute : ExportAttribute
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="contractType"></param>
        public NancyExportAttribute(Type contractType)
            : base(contractType)
        {

        }

    }

}
