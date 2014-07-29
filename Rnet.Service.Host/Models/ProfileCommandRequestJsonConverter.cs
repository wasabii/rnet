using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rnet.Service.Host.Models
{

    public class ProfileCommandRequestJsonConverter :
        JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ProfileCommandRequest);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JToken.ReadFrom(reader) as JObject;
            if (obj == null)
                return null;

            return new ProfileCommandRequest()
            {
                Parameters = ReadParameters(obj).ToArray(),
            };
        }

        IEnumerable<ProfileCommandParameter> ReadParameters(JObject jobj)
        {
            foreach (var property in jobj.Properties())
            {
                yield return new ProfileCommandParameter()
                {
                    Name = property.Name,
                    Value = property.Value.Value<object>(),
                };
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

    }

}
