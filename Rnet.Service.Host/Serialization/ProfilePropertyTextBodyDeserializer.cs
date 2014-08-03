using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using Rnet.Service.Host.Models;
using Rnet.Service.Host.Net;

namespace Rnet.Service.Host.Serialization
{

    [Export(typeof(IBodyDeserializer))]
    public class ProfilePropertyTextBodyDeserializer :
        IBodyDeserializer
    {

        public bool CanDeserialize(Type type, object target, MediaRange mediaRange)
        {
            return type == typeof(ProfilePropertyRequest) && mediaRange == "text/plain";
        }

        public object Deserialize(Type type, object target, Stream input)
        {
            var t = (ProfilePropertyRequest)target;
            var c = TypeDescriptor.GetConverter(t.Type);

            using (var rdr = new StreamReader(input))
                return new ProfilePropertyRequest() { Value = c.ConvertFromString(rdr.ReadToEnd()) };
        }

    }

}
