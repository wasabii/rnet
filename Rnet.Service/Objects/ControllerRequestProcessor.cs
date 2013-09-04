using System.ComponentModel.Composition;
using System.Threading.Tasks;

using Rnet.Service.Processors;

namespace Rnet.Service.Objects
{

    [RequestProcessor(typeof(RnetController))]
    public class ControllerRequestProcessor : DeviceRequestProcessor
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="module"></param>
        protected ControllerRequestProcessor(
            ObjectModule module)
            : base(module)
        {

        }

        /// <summary>
        /// Gets the target device.
        /// </summary>
        public RnetController Controller
        {
            get { return (RnetController)base.Device; }
        }

        public override async Task<object> Get()
        {
            return await Module.ControllerToData(Controller);
        }

    }

}
