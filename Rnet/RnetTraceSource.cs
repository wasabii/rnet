using System.Diagnostics;

namespace Rnet
{

    /// <summary>
    /// Provides methods and properties for Rnet components to log messages.
    /// </summary>
    class RnetTraceSource : TraceSource
    {

        /// <summary>
        /// Default <see cref="RnetTraceSource"/>.
        /// </summary>
        public static readonly RnetTraceSource Default = new RnetTraceSource();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetTraceSource()
            : base("Rnet")
        {

        }

    }

}
