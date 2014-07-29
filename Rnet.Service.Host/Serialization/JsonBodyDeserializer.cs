using System;
using System.ComponentModel.Composition;
using System.IO;

using Newtonsoft.Json;

using Rnet.Service.Host.Net;

namespace Rnet.Service.Host.Serialization
{

    /// <summary>
    /// Provides deserialization from JSON.
    /// </summary>
    [Export(typeof(IBodyDeserializer))]
    public class JsonDeserializer :
        IBodyDeserializer
    {

        /// <summary>
        /// Checks whether the given contentType is a JSON type.
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
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

        readonly Newtonsoft.Json.JsonSerializer serializer;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        [ImportingConstructor]
        public JsonDeserializer()
        {
            this.serializer = new Newtonsoft.Json.JsonSerializer();
            this.serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
        }

        public bool CanDeserialize(Type type, MediaRange mediaRange)
        {
            return IsJsonType(mediaRange);
        }

        public object Deserialize(Type type, Stream input)
        {
            using (var rdr = new JsonTextReader(new StreamReader(input)))
                return serializer.Deserialize(rdr, type);
        }

    }

}
