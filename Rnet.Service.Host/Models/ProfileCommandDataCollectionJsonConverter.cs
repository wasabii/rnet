using System;

using Newtonsoft.Json;

namespace Rnet.Service.Models
{

    public class ProfileCommandDataCollectionJsonConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ProfileCommandDataCollection);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            foreach (var command in (ProfileCommandDataCollection)value)
            {
                writer.WritePropertyName(command.Name);
                writer.WriteStartObject();
                writer.WritePropertyName("Href");
                writer.WriteValue(command.Uri);
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
        }

    }

}
