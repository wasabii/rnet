using System.ServiceProcess;

namespace Rnet.Service
{

    public partial class Service : ServiceBase
    {

        RnetHost host = new RnetHost();

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
