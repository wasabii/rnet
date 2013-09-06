using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Nancy;

namespace Rnet.Service.Serialization
{

    /// <summary>
    /// Provides serialization to XML, and adds formatting.
    /// </summary>
    [Export(typeof(ISerializer))]
    public class XmlSerializer : ISerializer
    {

        public static bool IsXmlType(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                return false;

            var str = contentType.Split(';')[0];
            if (str.Equals("application/xml", StringComparison.InvariantCultureIgnoreCase) ||
                str.Equals("text/xml", StringComparison.InvariantCultureIgnoreCase))
                return true;

            if (str.StartsWith("application/vnd", StringComparison.InvariantCultureIgnoreCase) &&
                str.EndsWith("+xml", StringComparison.InvariantCultureIgnoreCase))
                return true;

            return false;
        }

        public IEnumerable<string> Extensions
        {
            get { yield return "xml"; }
        }

        public bool CanSerialize(string contentType)
        {
            return IsXmlType(contentType);
        }

        public void Serialize<TModel>(string contentType, TModel model, Stream outputStream)
        {
            try
            {
                // output to temporary document
                var xml = new XDocument();
                var srs = new System.Xml.Serialization.XmlSerializer(model.GetType());

                // serialize
                using (var wrt = xml.CreateWriter())
                    srs.Serialize(wrt, model);

                // clean up
                xml.Descendants().Attributes().Where(i => i.IsNamespaceDeclaration).Remove();
                xml.Save(outputStream, SaveOptions.OmitDuplicateNamespaces);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

    }

}
