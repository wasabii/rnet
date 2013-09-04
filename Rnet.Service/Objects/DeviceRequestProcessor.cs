using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Rnet.Service.Processors;

namespace Rnet.Service.Objects
{

    [RequestProcessor(typeof(RnetDevice))]
    public class DeviceRequestProcessor : ObjectRequestProcessor
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        protected DeviceRequestProcessor(
            ObjectModule module)
            : base(module)
        {

        }

        /// <summary>
        /// Gets the target device.
        /// </summary>
        public RnetDevice Device
        {
            get { return (RnetDevice)base.Object; }
        }

        public override async Task<object> Get()
        {
            return await Module.DeviceToData(Device);
        }

    }

}
