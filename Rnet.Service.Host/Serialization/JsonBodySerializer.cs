using System;
using System.ComponentModel.Composition;
using System.IO;

using Newtonsoft.Json;

using Rnet.Service.Host.Net;

namespace Rnet.Service.Host.Serialization
{

    /// <summary>
    /// Provides serialization to JSON, and adds formatting.
    /// </summary>
    [Export(typeof(IBodySerializer))]
    public class JsonSerializer :
        IBodySerializer
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
        public JsonSerializer()
        {
            this.serializer = new Newtonsoft.Json.JsonSerializer();
            this.serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
        }

        public bool CanSerialize(object value, MediaRange mediaRange)
        {
            return IsJsonType(mediaRange);
        }

        public void Serialize(object value, MediaRange mediaRange, Stream output)
        {
            using (var jsonWriter = new JsonTextWriter(new StreamWriter(output)))
                serializer.Serialize(jsonWriter, value);
        }

    }

}
