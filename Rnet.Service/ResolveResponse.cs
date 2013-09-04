using System;
using System.Diagnostics.Contracts;

namespace Rnet.Service
{

    /// <summary>
    /// Describes the current target of a request, beginning at an object that corresponds to the first element in the path.
    /// </summary>
    public class ResolveResponse
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ResolveResponse(object o, string[] path)
            : base()
        {
            Contract.Requires<ArgumentNullException>(o != null);
            Contract.Requires<ArgumentNullException>(path != null);

            Object = o;
            Path = path;
        }

        /// <summary>
        /// Object that is targetted.
        /// </summary>
        public object Object { get; private set; }

        /// <summary>
        /// Path following the object.
        /// </summary>
        public string[] Path { get; private set; }

    }

}
