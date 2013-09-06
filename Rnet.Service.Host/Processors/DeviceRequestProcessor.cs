using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Linq;

using Rnet.Service.Processors;
using Rnet.Service.Models;

namespace Rnet.Service.Processors
{

    [RequestProcessor(typeof(RnetDevice), -75)]
    public class DeviceRequestProcessor : ObjectRequestProcessor<RnetDevice>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        [ImportingConstructor]
        protected DeviceRequestProcessor(
            BusModule module)
            : base(module)
        {
            Contract.Requires<ArgumentNullException>(module != null);
        }

        public override async Task<object> Resolve(RnetDevice target, string[] path)
        {
            // referring to a data path
            if (path[0] == Util.DATA_URI_SEGMENT)
                return await ResolveData(target, path);

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

            // list of all available data items
            if (path.Length == 1)
                return Task.FromResult<object>(ToDataCollection(device));

            return Task.FromResult<object>(ResolveDataItem(device, path[1]));
        }

        DataHandleCollection ToDataCollection(RnetDevice device)
        {
            return new DataHandleCollection(device.Data.Select(i => new DataHandleData()
            {
                Uri = device.GetUri(Context).UriCombine(Util.DATA_URI_SEGMENT).UriCombine(i.Path.ToString()),
                Path = i.Path.ToString(),
            }));
        }

        DataHandleData ResolveDataItem(RnetDevice device, string path)
        {
            return null;
        }

    }

}
