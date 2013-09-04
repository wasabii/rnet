using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml.Linq;

using Nancy;
using Nancy.Responses.Negotiation;

using Rnet.Drivers;
using Rnet.Profiles.Metadata;

namespace Rnet.Service.Objects
{

    [Export(typeof(IResponseProcessor))]
    public class ProfileXmlResponseProcessor : IResponseProcessor
    {

        public IEnumerable<Tuple<string, MediaRange>> ExtensionMappings
        {
            get
            {
                yield return new Tuple<string, MediaRange>("xml", "application/xml");
            }
        }

        public ProcessorMatch CanProcess(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            var m = new ProcessorMatch();

            if (model is Profile)
                m.ModelResult = MatchResult.ExactMatch;

            if (requestedMediaRange.Matches("*/xml"))
                m.RequestedContentTypeResult = MatchResult.ExactMatch;

            return m;
        }

        public Response Process(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            return new Response()
            {
                Contents = s => ProfileToXml(context, (Profile)model).Save(s, SaveOptions.OmitDuplicateNamespaces),
                ContentType = "application/xml",
            };
        }

        XDocument ProfileToXml(NancyContext context, Profile profile)
        {
            var md = profile.Metadata;
            var ns = md.XmlNamespace;

            return new XDocument(
                new XElement(ns + md.Name,
                    new XAttribute("Id", md.Id),
                    md.Properties.Select(i => PropertyToXml(context, profile, i))));
        }

        XElement PropertyToXml(NancyContext context, Profile profile, PropertyDescriptor property)
        {
            var md = profile.Metadata;
            var ns = md.XmlNamespace;

            return new XElement(ns + property.Name,
                new XAttribute("Href", property.Name),
                new XElement(ns + "Value",
                    property.GetValue(profile.Instance)));
        }

    }

}
