using System;
using System.ServiceModel.Web;

namespace Rnet.Service
{

    abstract class WebServiceBase
    {

        RnetBus bus;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="bus"></param>
        protected WebServiceBase(RnetBus bus)
        {
            this.bus = bus;
        }

        /// <summary>
        /// Gets the <see cref="RnetBus"/>.
        /// </summary>
        protected RnetBus Bus
        {
            get { return bus; }
        }

        /// <summary>
        /// Gets the current incoming web request.
        /// </summary>
        /// <returns></returns>
        protected static IncomingWebRequestContext IncomingRequest
        {
            get { return WebOperationContext.Current.IncomingRequest; }
        }

        /// <summary>
        /// Gets the base URI of the current request.
        /// </summary>
        /// <returns></returns>
        protected static Uri BaseUri
        {
            get { return IncomingRequest.UriTemplateMatch.BaseUri; }
        }

        /// <summary>
        /// Converts the given <see cref="RnetDeviceId"/> into a string.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected static string DeviceIdToString(RnetDeviceId id)
        {
            return string.Format("{0}.{1}.{2}", (int)id.ControllerId, (int)id.ZoneId, (int)id.KeypadId);
        }

    }

}
