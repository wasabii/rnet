using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Rnet.Service
{

    class RnetWebHttpBehavior : WebHttpBehavior
    {

        protected override IDispatchMessageFormatter GetReplyDispatchFormatter(OperationDescription operationDescription, ServiceEndpoint endpoint)
        {
            return base.GetReplyDispatchFormatter(operationDescription, endpoint);
        }

    }

}
