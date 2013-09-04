using System;
using System.ComponentModel.Composition;

namespace Rnet.Service.Processors
{

    [AttributeUsage(AttributeTargets.Class)]
    public class RequestProcessorAttribute : ExportAttribute
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RequestProcessorAttribute()
            :base(typeof(IRequestProcessor))
        {

        }

    }

}
