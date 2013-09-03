using System;
using System.Linq;

using Newtonsoft.Json;

namespace Rnet.Service.Objects
{

    class ProfileRefCollectionJsonConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ProfileRefCollection);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var o = (ProfileRefCollection)value;
            serializer.Serialize(writer, o.ToDictionary(i => i.Id, i => i.Href));
        }

    }

}
