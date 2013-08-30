using System.ServiceModel.Web;
using System.ServiceProcess;

namespace Rnet.Service
{

    public partial class Service : ServiceBase
    {

        ServiceHost host = new ServiceHost();

        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            host = new ServiceHost();
            host.OnStart();
        }

        protected override void OnStop()
        {
            host.OnStop();
        }

    }

}
