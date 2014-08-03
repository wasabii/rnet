using System.ComponentModel.Composition;
using System.IO;

using Rnet.Service.Host.Models;
using Rnet.Service.Host.Net;

namespace Rnet.Service.Host.Serialization
{

    [Export(typeof(IBodySerializer))]
    public class ProfilePropertyTextBodySerializer :
        IBodySerializer
    {

        public bool CanSerialize(object value, MediaRange mediaRange)
        {
            return value is ProfilePropertyData && mediaRange == "text/plain";
        }

        public void Serialize(object value, MediaRange mediaRange, Stream output)
        {
            var property = value as ProfilePropertyData;
            if (property == null)
                return;

            var obj = property.Value;
            if (obj == null)
                return;

            using (var wrt = new StreamWriter(output))
                wrt.Write(obj);
        }

    }

}
