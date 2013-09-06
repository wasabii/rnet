using System;
using Rnet.Service.Host;

namespace Rnet.Service
{

    class ServiceImpl : IDisposable
    {

        RnetHost host;
        RnetBus rnet;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ServiceImpl()
        {

        }

        protected override void OnStart(string[] args)
        {
            rnet = new RnetBus(@"rnet.tcp://tokyo.cogito.cx");
            rnet.Start(new Nito.As);

            host = new RnetHost(, "http://localhost:12292/rnet");
            host.Start();
        }

        protected override void OnStop()
        {
            host.Stop();
            host = null;
        }

    }

}
