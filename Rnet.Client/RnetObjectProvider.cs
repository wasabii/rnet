using System;
using System.Collections.Generic;
using System.Composition;
using System.Net.Http;

using Rnet.Util;

namespace Rnet.Client.Http
{

    public class RnetObjectProvider :
        IRnetObjectProvider
    {

        readonly HttpClient http;
        readonly Dictionary<Uri, RnetRef> objects;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RnetObjectProvider()
        {
            this.http = new HttpClient();
            this.http.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            this.objects = new Dictionary<Uri, RnetRef>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public RnetRef GetAsync(Uri uri)
        {
            lock (objects)
                return objects.GetOrCreate(uri, i => new RnetRef(uri));
        }

    }

}
