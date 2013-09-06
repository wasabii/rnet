using System.ServiceProcess;

using Rnet.Service.Host.Host;

namespace Rnet.Service
{

    public partial class Service : ServiceBase
    {

        static readonly RnetHost web;
        static readonly RnetBus rnet;

        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            host = new RnetHost();
            host.Start();
        }

        protected override void OnStop()
        {
            host.Stop();
        }

    }

}
