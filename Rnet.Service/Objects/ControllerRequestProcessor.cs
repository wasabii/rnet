using System.ComponentModel.Composition;

using Rnet.Service.Processors;

namespace Rnet.Service.Objects
{

    [RequestProcessor(typeof(RnetController), -50)]
    public class ControllerRequestProcessor : DeviceRequestProcessor
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        [ImportingConstructor]
        protected ControllerRequestProcessor(
            ObjectModule module)
            : base(module)
        {

        }

    }

}
