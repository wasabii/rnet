using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;

namespace Rnet.Service
{

    /// <summary>
    /// Applied to a web service. Implements various abilities we want.
    /// </summary>
    public class FormatServiceBehavior : Attribute, IServiceBehavior, IOperationBehavior, IParameterInspector
    {

        void IServiceBehavior.AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {

        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (var ep in serviceDescription.Endpoints)
                foreach (var op in ep.Contract.Operations)
                    op.OperationBehaviors.Insert(0, this);
        }

        void IServiceBehavior.Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {

        }

        void IOperationBehavior.AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {

        }

        void IOperationBehavior.ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {

        }

        void IOperationBehavior.ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            dispatchOperation.ParameterInspectors.Add(this);
        }

        void IOperationBehavior.Validate(OperationDescription operationDescription)
        {

        }

        object IParameterInspector.BeforeCall(string operationName, object[] inputs)
        {
            //var ctx = WebOperationContext.Current;
            //if (ctx == null)
            //    return;

            //switch (ctx.IncomingRequest.UriTemplateMatch.QueryParameters["format"])
            //{
            //    case "xml":
            //        ctx.OutgoingResponse.Format = WebMessageFormat.Xml;
            //        break;
            //    case "json":
            //        ctx.OutgoingResponse.Format = WebMessageFormat.Json;
            //        break;
            //    default:
            //        break;
            //}

            return null;
        }

        void IParameterInspector.AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        {

        }

    }

}
