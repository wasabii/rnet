using System;
using System.Diagnostics.Contracts;

namespace Rnet.Client
{

    /// <summary>
    /// Provides a persistent client instance to the RNET JSON interface.
    /// </summary>
    public class RnetClient
    {

        readonly Uri uri;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="uri"></param>
        public RnetClient(Uri uri)
        {
            Contract.Requires<ArgumentNullException>(uri != null);

            this.uri = uri;
        }

        /// <summary>
        /// Gets the URI of the RNET interface.
        /// </summary>
        public Uri Uri
        {
            get { return uri; }
        }

    }

}
