using System;
using System.Diagnostics.Contracts;

namespace Rnet.Client
{

    /// <summary>
    /// Provides an base class for a reference to a remote RNET object.
    /// </summary>
    public abstract class RnetRef
    {

        readonly Uri uri;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="uri"></param>
        public RnetRef(Uri uri)
        {
            Contract.Requires<ArgumentNullException>(uri != null);

            this.uri = uri;
        }

    }

}
