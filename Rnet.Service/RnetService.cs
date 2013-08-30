using System.ServiceModel.Web;
using System.ServiceProcess;

namespace Rnet.Service
{

    public partial class RnetService : ServiceBase
    {

        ServiceHost host = new ServiceHost();

        public RnetService()
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
