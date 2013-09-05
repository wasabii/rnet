using System;
using System.Diagnostics.Contracts;

using Nancy;

namespace Rnet.Service
{

    static class Util
    {

        /// <summary>
        /// Prefix to be prepended to the profile path.
        /// </summary>
        public static readonly string PROFILE_URI_PREFIX = "~";

        /// <summary>
        /// Segment under a device which 
        /// </summary>
        public static readonly string DATA_URI_SEGMENT = "_data";

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

        /// <summary>
        /// Gets the base URI for the given <see cref="NancyContext"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Uri GetBaseUri(this NancyContext context)
        {
            var u = context.Request.Url.Clone();
            u.Path = context.NegotiationContext.ModulePath;
            return u;
        }

    }

}
