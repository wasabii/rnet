using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Rnet.Service.Processors;

namespace Rnet.Service.Objects
{

    [RequestProcessor(typeof(RnetDevice), -75)]
    public class DeviceRequestProcessor : ObjectRequestProcessor
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        [ImportingConstructor]
        protected DeviceRequestProcessor(
            ObjectModule module)
            : base(module)
        {

        }

        public override async Task<object> Resolve(RnetBusObject target, string[] path)
        {
            // referring to a data path
            if (path[0] == Util.DATA_URI_SEGMENT)
                return await ResolveData((RnetDevice)target, path);

            return await base.Resolve(target, path);
        }

        /// <summary>
        /// Resolves a data handle for from the device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="path"></param>
        /// <param name="dataPath"></param>
        /// <returns></returns>
        Task<object> ResolveData(RnetDevice device, string[] path)
        {
            Contract.Requires<ArgumentNullException>(device != null);
            Contract.Requires<ArgumentNullException>(path != null);

            if (path.Length == 1)
                return Task.FromResult<object>(device.Data);

            return Task.FromResult<object>(device[path[1]]);
        }

    }

}
