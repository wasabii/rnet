using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Rnet.Service.Objects
{

    class ObjectDataCollectionJsonConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return objectType is IEnumerable<ObjectData>;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var objects = (IEnumerable<ObjectData>)value;
            
            writer.WriteStartObject();
            foreach (var o in objects)
            {
                writer.WritePropertyName(o.Id);
                serializer.Serialize(writer, o);
            }
            writer.WriteEndObject();
        }

    }

}
