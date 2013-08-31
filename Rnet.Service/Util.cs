using System;
using System.Diagnostics.Contracts;

namespace Rnet.Service
{

    static class Util
    {

        /// <summary>
        /// Combines the relative Uri into the base Uri.
        /// </summary>
        /// <param name="baseUri"></param>
        /// <param name="relativeUri"></param>
        /// <returns></returns>
        public static Uri UriCombine(this Uri baseUri, string relativeUri)
        {
            Contract.Requires<ArgumentNullException>(baseUri != null);
            Contract.Requires<ArgumentNullException>(relativeUri != null);

            return new Uri(new Uri(baseUri.ToString().TrimEnd('/') + "/"), relativeUri.Trim('/'));
        }

    }

}
