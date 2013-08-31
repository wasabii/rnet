using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Mime;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;

using Rnet.Service.Devices;

namespace Rnet.Service
{

    /// <summary>
    /// Serves as the base class for services.
    /// </summary>
    abstract class WebServiceBase
    {

        /// <summary>
        /// Gets the current <see cref="WebOperationContext"/>.
        /// </summary>
        /// <returns></returns>
        protected static WebOperationContext Context
        {
            get { return WebOperationContext.Current; }
        }

        /// <summary>
        /// Gets the current incoming web request.
        /// </summary>
        /// <returns></returns>
        protected static IncomingWebRequestContext IncomingRequest
        {
            get { return Context.IncomingRequest; }
        }

        /// <summary>
        /// Gets the current outgoing web response.
        /// </summary>
        /// <returns></returns>
        protected static OutgoingWebResponseContext OutgoingResponse
        {
            get { return Context.OutgoingResponse; }
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
        protected static string GetDeviceIdAsString(RnetDeviceId id)
        {
            return string.Format("{0}.{1}.{2}", (int)id.ControllerId, (int)id.ZoneId, (int)id.KeypadId);
        }

        /// <summary>
        /// Converts the given <see cref="RnetPath"/> into a string.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected static string GetPathAsString(RnetPath path)
        {
            return path.ToString();
        }

        /// <summary>
        /// Gets the URL for the given <see cref="RnetDevice"/>.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        protected static Uri GetDeviceUri(RnetDevice device)
        {
            return new Uri(BaseUri, string.Format("{0}.{1}.{2}",
                (int)device.DeviceId.ControllerId,
                (int)device.DeviceId.ZoneId,
                (int)device.DeviceId.KeypadId));
        }

        /// <summary>
        /// Transforms a <see cref="RnetDevice"/> into a <see cref="Device"/>.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        protected static Device RnetDeviceToInfo(RnetDevice device)
        {
            Contract.Requires<ArgumentNullException>(device != null);

            var controller = device as RnetController;
            if (controller != null)
                return new Controller()
                {
                    Id = GetDeviceUri(device),
                    DeviceId = GetDeviceIdAsString(device.DeviceId),
                };

            return new Device()
            {
                Id = GetDeviceUri(device),
                DeviceId = GetDeviceIdAsString(device.DeviceId),
            };
        }

        RnetBus bus;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="bus"></param>
        protected WebServiceBase(RnetBus bus)
        {
            Contract.Requires<ArgumentNullException>(bus != null);

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
        /// Creates an appropriate response message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        protected Message CreateResponse<T>(T result)
        {
            if (result == null)
                return null;

            // accepted content types
            var contentTypes = new List<ContentType>(IncomingRequest.GetAcceptHeaderElements());

            // specifically requested content types
            switch (IncomingRequest.UriTemplateMatch.QueryParameters["format"] ?? "")
            {
                case "xml":
                    contentTypes.Insert(0, new ContentType("application/xml"));
                    break;
                case "json":
                    contentTypes.Insert(0, new ContentType("application/json"));
                    break;
            }

            // find first supported type
            foreach (var contentType in contentTypes)
            {
                switch (contentType.MediaType)
                {
                    case "text/xml":
                    case "application/xml":
                        return Context.CreateXmlResponse<T>(result);
                    case "text/json":
                    case "application/json":
                        return Context.CreateJsonResponse<T>(result);
                }
            }

            // binary result
            if (typeof(byte[]).IsInstanceOfType(result))
                return Context.CreateStreamResponse(new MemoryStream((byte[])(object)result), "application/octet-stream");

            // stream result
            if (typeof(Stream).IsInstanceOfType(result))
                return Context.CreateStreamResponse((Stream)(object)result, "application/octet-stream");

            // by default return XML
            return Context.CreateXmlResponse<T>(result);
        }

    }

}
