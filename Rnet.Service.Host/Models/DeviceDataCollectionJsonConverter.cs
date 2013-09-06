using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Rnet.Service.Host.Models
{

    class DeviceDataCollectionJsonConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return objectType is IEnumerable<DeviceData>;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var devices = (IEnumerable<DeviceData>)value;
            
            writer.WriteStartObject();
            foreach (var o in devices)
            {
                writer.WritePropertyName(o.RnetId);
                serializer.Serialize(writer, o);
            }
            writer.WriteEndObject();
        }

    }

}
