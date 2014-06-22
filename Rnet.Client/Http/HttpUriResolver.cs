using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rnet.Client.Http
{

    public class HttpUriResolver :
        IUrlResolver
    {

        readonly HttpClient http;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public HttpUriResolver()
        {
            this.http = new HttpClient();
            this.http.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        }

        public async Task<JObject> GetAsync(Uri uri)
        {
            using (var rdr = new JsonTextReader(new StreamReader(await http.GetStreamAsync(uri))))
                return JObject.Load(rdr);
        }

    }

}
