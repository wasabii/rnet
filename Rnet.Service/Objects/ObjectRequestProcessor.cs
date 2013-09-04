using System.Threading.Tasks;

using Rnet.Service.Processors;

namespace Rnet.Service.Objects
{

    /// <summary>
    /// Handles requests for <see cref="RnetBusObject"/> instances.
    /// </summary>
    [RequestProcessor(typeof(RnetBusObject))]
    public class ObjectRequestProcessor : RequestProcessor<RnetBusObject>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        protected ObjectRequestProcessor(
            ObjectModule module)
            : base(module)
        {

        }

        public override async Task<object> Get()
        {
            return await Module.ObjectToData(Object);
        }

    }

}
