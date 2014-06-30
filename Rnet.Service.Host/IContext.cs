using System.Collections.Generic;

using Microsoft.Owin;

namespace Rnet.Service.Host
{

    /// <summary>
    /// Exposes the request context.
    /// </summary>
    public interface IContext
    {

        T Get<T>(string key);

        IContext Set<T>(string key, T value);

        IOwinRequest Request { get; }

        IOwinResponse Response { get; }

    }

}
