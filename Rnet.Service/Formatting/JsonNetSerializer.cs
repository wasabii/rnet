using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;

using Nancy;
using Nancy.IO;

using Newtonsoft.Json;

namespace Rnet.Service.Formatting
{

    /// <summary>
    /// Provides serialization to JSON, and adds formatting.
    /// </summary>
    [Export(typeof(ISerializer))]
    public class JsonNetSerializer : ISerializer
    {

        public static bool IsJsonType(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                return false;

            var str = contentType.Split(';')[0];
            if (str.Equals("application/json", StringComparison.InvariantCultureIgnoreCase) ||
                str.Equals("text/json", StringComparison.InvariantCultureIgnoreCase))
                return true;

            if (str.StartsWith("application/vnd", StringComparison.InvariantCultureIgnoreCase) &&
                str.EndsWith("+json", StringComparison.InvariantCultureIgnoreCase))
                return true;

            return false;
        }

        readonly JsonSerializer serializer;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        [ImportingConstructor]
        public JsonNetSerializer()
        {
            this.serializer = new JsonSerializer();
            this.serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
        }

        public IEnumerable<string> Extensions
        {
            get { yield return "json"; }
        }

        public bool CanSerialize(string contentType)
        {
            return IsJsonType(contentType);
        }

        public void Serialize<TModel>(string contentType, TModel model, Stream outputStream)
        {
            using (var jsonTextWriter = new JsonTextWriter(new StreamWriter(new UnclosableStreamWrapper(outputStream))))
                serializer.Serialize(jsonTextWriter, model);
        }

    }

}
