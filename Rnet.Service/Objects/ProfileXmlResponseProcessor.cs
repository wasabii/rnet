using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using System.Xml.Serialization;

using Nancy;
using Nancy.Responses.Negotiation;

namespace Rnet.Service.Objects
{

    /// <summary>
    /// Implements XML serialization of the <see cref="ProfileHandler"/> and supporting types.
    /// </summary>
    [Export(typeof(IResponseProcessor))]
    public class ProfileXmlResponseProcessor : IResponseProcessor
    {

        public IEnumerable<Tuple<string, MediaRange>> ExtensionMappings
        {
            get { yield return new Tuple<string, MediaRange>("xml", "application/xml"); }
        }

        public ProcessorMatch CanProcess(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            var m = new ProcessorMatch();
            return m;
            if (model is ProfileData)
            //||
            //    model is ProfilePropertyData ||
            //    model is ProfileCommandData)
                m.ModelResult = MatchResult.ExactMatch;

            if (requestedMediaRange.Matches("*/xml"))
                m.RequestedContentTypeResult = MatchResult.ExactMatch;

            return m;
        }

        public Response Process(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            if (model is ProfileData)
                return Process((ProfileData)model, context);

            //if (model is ProfilePropertyData)
            //    return Process((ProfilePropertyData)model, context);

            //if (model is ProfileCommandData)
            //    return Process((ProfileCommandData)model, context);

            return new Response()
            {
                StatusCode = HttpStatusCode.NotImplemented,
            };
        }

        Response Process(ProfileData profile, NancyContext context)
        {
            return new Response()
            {
                ContentType = "application/xml",
                Contents = s => ProfileToXml(context, profile).Save(s, SaveOptions.OmitDuplicateNamespaces),
            };
        }

        //Response Process(ProfilePropertyData property, NancyContext context)
        //{
        //    return new Response()
        //    {
        //        ContentType = "application/xml",
        //        Contents = s => PropertyToXml(context, property).Save(s, SaveOptions.OmitDuplicateNamespaces),
        //    };
        //}

        //Response Process(ProfileCommandData command, NancyContext context)
        //{
        //    return new Response()
        //    {
        //        ContentType = "application/xml",
        //        Contents = s => CommandToXml(context, command).Save(s, SaveOptions.OmitDuplicateNamespaces),
        //    };
        //}

        XDocument ProfileToXml(NancyContext context, ProfileData profile)
        {
            var xml = new XDocument();
            var srs = new XmlSerializer(typeof(ProfileData));
            using (var wrt = xml.CreateWriter())
                srs.Serialize(wrt, profile);

            return xml;
           // return new XDocument(xml.Root.FirstNode);
        }

        //XElement PropertyToXml(NancyContext context, ProfilePropertyData property)
        //{

        //}

        //XElement CommandToXml(NancyContext context, ProfilePropertyData property)
        //{

        //}

    }

}
