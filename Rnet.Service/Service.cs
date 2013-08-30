using System.ServiceModel.Web;
using System.ServiceProcess;

namespace Rnet.Service
{

    public partial class Service : ServiceBase
    {

        Host host = new Host();

        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            host = new Host();
            host.OnStart();
        }

        protected override void OnStop()
        {
            host.OnStop();
        }

    }

}
