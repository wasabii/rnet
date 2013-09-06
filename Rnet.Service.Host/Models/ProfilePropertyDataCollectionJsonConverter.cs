using System;
using System.Linq;

using Newtonsoft.Json;

namespace Rnet.Service.Models
{

    public class ProfilePropertyDataCollectionJsonConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ProfilePropertyDataCollection);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            foreach (var property in (ProfilePropertyDataCollection)value)
            {
                writer.WritePropertyName(property.Name);
                writer.WriteStartObject();
                writer.WritePropertyName("Href");
                writer.WriteValue(property.Uri);
                writer.WritePropertyName("Value");
                serializer.Serialize(writer, property.Value);
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
        }

    }

}
