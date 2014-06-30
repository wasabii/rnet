using System;
using System.Diagnostics.Contracts;
using Microsoft.Owin;

namespace Rnet.Service.Host
{

    public class OwinContext :
        IContext
    {

        readonly IOwinContext owin;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="owin"></param>
        public OwinContext(IOwinContext owin)
        {
            Contract.Requires<ArgumentNullException>(owin != null);

            this.owin = owin;
        }

        public T Get<T>(string key)
        {
            return owin.Get<T>(key);
        }

        public IOwinRequest Request
        {
            get { return owin.Request; }
        }

        public IOwinResponse Response
        {
            get { return owin.Response; }
        }

        public IContext Set<T>(string key, T value)
        {
            return Set<T>(key, value);
        }

    }

}
