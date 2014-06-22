using System;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace Rnet.Client
{

    /// <summary>
    /// Provides the ability to request data from an RNET Uri.
    /// </summary>
    public interface IUrlResolver
    {

        /// <summary>
        /// Gets the specified URI.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        Task<JObject> GetAsync(Uri uri);

    }

}
