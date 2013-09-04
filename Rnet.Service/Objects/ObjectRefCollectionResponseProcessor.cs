using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Nancy;
using Nancy.Responses.Negotiation;

namespace Rnet.Service.Objects
{

    [Export(typeof(IResponseProcessor))]
    public class ObjectRefCollectionResponseProcessor : IResponseProcessor
    {

        public IEnumerable<Tuple<string, MediaRange>> ExtensionMappings
        {
            get { throw new NotImplementedException(); }
        }

        public ProcessorMatch CanProcess(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            throw new System.NotImplementedException();
        }

        public Response Process(MediaRange requestedMediaRange, dynamic model, Nancy.NancyContext context)
        {
            throw new NotImplementedException();
        }

    }

}
