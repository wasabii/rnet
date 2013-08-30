using System;

namespace Rnet.Service
{

    static class Util
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseUri"></param>
        /// <param name="relativeUri"></param>
        /// <returns></returns>
        public static Uri UriCombine(this Uri baseUri, string relativeUri)
        {
            return new Uri(new Uri(baseUri.ToString().TrimEnd('/') + "/"), relativeUri.Trim('/'));
        }

    }

}
