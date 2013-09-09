using System.ServiceProcess;

namespace Rnet.Service
{

    public partial class Service : ServiceBase
    {

        ServiceImpl impl;

        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            impl = new ServiceImpl();
            impl.OnStart(args);
        }

        protected override void OnStop()
        {
            impl.OnStop();
            impl = null;
        }

    }

}
