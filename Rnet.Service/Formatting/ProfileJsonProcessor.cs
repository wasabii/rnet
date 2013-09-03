using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using Nancy;
using Nancy.Responses;
using Nancy.Responses.Negotiation;

using Rnet.Drivers;
using Rnet.Service.Objects;

namespace Rnet.Service.Formatting
{

    [Export(typeof(IResponseProcessor))]
    public class ProfileJsonProcessor : IResponseProcessor
    {

        public IEnumerable<Tuple<string, MediaRange>> ExtensionMappings
        {
            get
            {
                yield return new Tuple<string, MediaRange>("json", "application/json");
            }
        }

        public ProcessorMatch CanProcess(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            var m = new ProcessorMatch();

            if (model is Profile)
                m.ModelResult = MatchResult.ExactMatch;

            if (requestedMediaRange.Matches("*/json"))
                m.RequestedContentTypeResult = MatchResult.ExactMatch;

            return m;
        }

        public Response Process(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            var profile = (Profile)model;

            return new JsonResponse<ProfileData>(new ProfileData()
            {
                Id = profile.Metadata.Id,
                Properties = new ProfilePropertyDataCollection(profile.Metadata.Properties.Select(i => new ProfilePropertyData()
                {
                    Href = new Uri(i.Name, UriKind.Relative),
                    Name = i.Name,
                    Value = i.GetValue(profile.Instance),
                }))
            }, new JsonSerializer());
        }

    }

}
