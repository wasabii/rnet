using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Microsoft.Owin;

using Rnet.Service.Host.Net;
using Rnet.Service.Host.Serialization;

namespace Rnet.Service.Host.Processors
{

    /// <summary>
    /// Defaults to serializing the object to the response.
    /// </summary>
    [ResponseProcessor(typeof(object), int.MinValue)]
    public class DefaultResponseProcessor :
        IResponseProcessor
    {

        readonly IEnumerable<IBodySerializer> serializers;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="serializers"></param>
        [ImportingConstructor]
        public DefaultResponseProcessor(
            [ImportMany] IEnumerable<IBodySerializer> serializers)
        {
            Contract.Requires<ArgumentNullException>(serializers != null);

            this.serializers = serializers;
        }

        public Task<bool> Handle(IContext context, object target)
        {
            foreach (var mediaRange in MediaRangeList.Parse(context.Request.Accept) + "application/json")
            {
                foreach (var serializer in serializers)
                {
                    if (serializer.CanSerialize(target, mediaRange))
                    {
                        // serialize object to response
                        context.Response.ContentType = mediaRange;
                        serializer.Serialize(target, mediaRange, context.Response.Body);

                        // finished response
                        return Task.FromResult(true);
                    }
                }
            }

            return Task.FromResult(false);
        }

    }

}
