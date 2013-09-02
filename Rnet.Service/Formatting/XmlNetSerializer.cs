using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

using Nancy;
using Nancy.IO;

namespace Rnet.Service.Formatting
{

    /// <summary>
    /// Provides serialization to XML using DataContract serialization.
    /// </summary>
    [Export(typeof(ISerializer))]
    public class XmlNetSerializer : ISerializer
    {

        public static bool IsXmlType(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                return false;

            var str = contentType.Split(';')[0];
            if (str.Equals("application/xml", StringComparison.InvariantCultureIgnoreCase) ||
                str.Equals("text/xml", StringComparison.InvariantCultureIgnoreCase))
                return true;

            return false;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        [ImportingConstructor]
        public XmlNetSerializer()
        {

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
            var serializer = new DataContractSerializer(model.GetType());
            using (var wrt = new XmlTextWriter(new StreamWriter(new UnclosableStreamWrapper(outputStream))))
            {
                wrt.Formatting = System.Xml.Formatting.Indented;
                serializer.WriteObject(wrt, model);
            }
        }

    }

}
