using System;
using System.Linq;

using Newtonsoft.Json;

namespace Rnet.Service.Objects
{

    class ObjectRefCollectionJsonConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ObjectRefCollection);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var o = (ObjectRefCollection)value;
            serializer.Serialize(writer, o.ToDictionary(i => i.Name, i => i.Href));
        }

    }

}
